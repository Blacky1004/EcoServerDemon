using System;
using System.Collections.Generic;
using System.IO;
//using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace EcoServerDemon.Classes
{
    public class Scheduler
    {
        private Thread _schedulerThread = null;
        private bool _isRunning = false;
        private LoaderData datas = null;
        private WorkerData _restarts = null;
        public Scheduler(LoaderData datas)
        {
            this.datas = datas;
            _restarts = new WorkerData();
            _restarts.RestartUpdates = new List<WorkerData.StarterData>();


            foreach(var restart in datas.restarts)
            {
                var available = _restarts.RestartUpdates.Find(f => f.RunTime.Equals(restart));
                if (available == null)
                    _restarts.RestartUpdates.Add(new WorkerData.StarterData()
                    {
                        RunTime = restart,
                        Type = RestarterType.RESTART
                    });

            }

            foreach(var update in datas.updates)
            {
                var available = _restarts.RestartUpdates.Find(f => f.RunTime.Equals(update));
                if(available != null)
                {
                    if(available.Type == RestarterType.RESTART)
                    {
                        _restarts.RestartUpdates.Where(f => f.RunTime.Equals(update)).ToList().ForEach(f => f.Type = RestarterType.UPDATE);
                    }
                }
                else
                {
                    _restarts.RestartUpdates.Add(new WorkerData.StarterData()
                    {
                        RunTime = update,
                        Type = RestarterType.UPDATE
                    });
                }
            }

            Console.WriteLine("confirm " + _restarts.RestartUpdates.Where(f=>f.Type == RestarterType.RESTART).Count() + " restarts");
            Console.WriteLine("confirm " + _restarts.RestartUpdates.Where(f => f.Type == RestarterType.UPDATE).Count() + " updates");
            List<DateTime> runDates = new List<DateTime>();
            foreach( var res in _restarts.RestartUpdates)
            {
                var resDate = Convert.ToDateTime(res.RunTime);
                if(resDate < DateTime.Now)
                {
                    resDate = resDate.AddDays(1);
                }
                runDates.Add(resDate);
            }
            var nextRuns = runDates.Where(w => w > DateTime.Now).Min();
            Console.WriteLine("next job is running at "+nextRuns.ToString("dd.MM.yyyy HH:mm"));
        }

        public void Start()
        {
            if(_schedulerThread != null)
            {
                _isRunning = false;
                if(_schedulerThread.ThreadState != ThreadState.Aborted)
                {
                    _schedulerThread.Abort();
                    _schedulerThread.Join();
                }

                _schedulerThread = null;

            }

            _schedulerThread = new Thread(new ThreadStart(DoWork));
            _schedulerThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            if(_schedulerThread != null)
            {
                if(_schedulerThread.ThreadState != ThreadState.Aborted)
                {
                    _schedulerThread.Abort();
                    _schedulerThread.Join();
                }
            }

            _schedulerThread = null;
        }

        private void DoWork()
        {
            _isRunning = true;

            while (_isRunning)
            {
                string formatString = "dd.MM.yyyy HH:mm";
                foreach(var rsTime in _restarts.RestartUpdates)
                {
                    var runTime = Convert.ToDateTime(rsTime.RunTime).ToString(formatString);
                    var nowTime = DateTime.Now.ToString(formatString);
                    if(runTime == nowTime)
                    {
                        Console.WriteLine("stopping EcoServer for "+ (rsTime.Type == RestarterType.RESTART ? "RESTART" : "UPDATE") + "...");
                        System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("EcoServer");
                        if (process == null)
                        {
                            //Server überhaupt gestartet?
                        }
                        else
                        {
                            for (int i = 0; i < process.Length; i++)
                            {
                                var ecoServer = process[i];
                                var execPath = ServerExecutePath(ecoServer);
                                var cfgServerPath = Path.Combine(datas.server_path, "EcoServer.exe");
                                if (execPath.ToLower().Equals(cfgServerPath.ToLower()))
                                {
                                    var consoleId = ServerConsoleProcessID(ecoServer.Id);
                                    if (!String.IsNullOrEmpty(consoleId))
                                    {
                                        int cID;
                                        if (int.TryParse(consoleId, out cID))
                                        {
                                            var consoleProcess = System.Diagnostics.Process.GetProcessById(cID);
                                            if (consoleProcess != null)
                                            {
                                                consoleProcess.Kill();
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        Console.WriteLine("EcoServer stopped.");
                        switch (rsTime.Type)
                        {
                            default:
                            case RestarterType.RESTART:
                                {
                                    Console.WriteLine("restart start ecoserver...");
                                    System.Diagnostics.Process restartProcess = System.Diagnostics.Process.Start(Path.Combine(datas.server_path, "EcoServer.exe"));
                                }
                                break;
                            case RestarterType.UPDATE:
                                {
                                    Console.WriteLine("update ecoserver...");
                                    var steamCmd = Path.Combine(datas.steam.cmd_path, "steamcmd.exe");
                                    if (File.Exists(steamCmd))
                                    {
                                        var steamProcess = new System.Diagnostics.Process();
                                        steamProcess.StartInfo = new System.Diagnostics.ProcessStartInfo()
                                        {
                                            Arguments = string.Format(" +login {0} {1} +force_install_dir {2} +app_update 739590 +quit", datas.steam.username, datas.steam.password, datas.server_path),
                                            FileName = steamCmd
                                        };
                                        steamProcess.Start();
                                        while (!steamProcess.HasExited) Thread.Sleep(10);
                                        Console.WriteLine("start updated ecoserver...");
                                        System.Diagnostics.Process restartProcess = System.Diagnostics.Process.Start(Path.Combine(datas.server_path, "EcoServer.exe"));

                                    }
                                }
                                break;
                        }
                    }
                }

                Thread.Sleep(60000);
            }
        }

        static private string ServerExecutePath(System.Diagnostics.Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch
            {
                string query = "SELECT * FROM Win32_Process";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach( ManagementObject item in searcher.Get())
                {
                    object id = item["ProcessID"];
                    object path = item["ExecutablePath"];
                    object commandLine = item["CommandLine"];
                    object name = item["Name"];
                    if (path != null && id.ToString() == process.Id.ToString())
                        return path.ToString();
                }
                
            }
            return "";
        }

        static private string ServerConsoleProcessID(int serverID)
        {
            string query = "SELECT ParentProcessId, ProcessID FROM Win32_Process";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject item in searcher.Get())
            {
                if (item["ParentProcessID"].ToString() == serverID.ToString())
                {
                    return item["ProcessID"].ToString();
                }
            }
            return "";
        }
    }

    public class WorkerData
    {
        public List<StarterData> RestartUpdates { get; set; }
        public class StarterData
        {
            public string RunTime { get; set; }

            public RestarterType Type { get; set; }
        }
    }

    public enum RestarterType
    {
        RESTART = 0,
        UPDATE = 1
    }
}
