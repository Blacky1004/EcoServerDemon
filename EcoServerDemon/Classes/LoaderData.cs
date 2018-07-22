using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoServerDemon.Classes
{
    public class LoaderData
    {
        public string server_path { get; set; }

        public List<string> restarts { get; set; }

        public List<string> updates { get; set; }

        public SteamData steam { get; set; }
    }

    public class SteamData
    {
        public string cmd_path { get; set; }

        public string username { get; set; }

        public string password { get; set; }
    }
}
