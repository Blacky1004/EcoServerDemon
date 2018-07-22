using EcoServerDemon.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EcoServerDemon
{
    class Program
    {
        private static readonly string _loaderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static void Main(string[] args)
        {



            var loaderFile = Path.Combine(_loaderPath, "ecoloader.json");
            if (!File.Exists(loaderFile))
                throw new FileNotFoundException("ecoloader.json not found!");
            var loaderContent = File.ReadAllText(loaderFile);
            if (String.IsNullOrEmpty(loaderContent))
                throw new FormatException("ecoloader.json has a wrong format!");
            var loaderData = JsonConvert.DeserializeObject<LoaderData>(loaderContent);
            if (loaderData == null)
                throw new Exception("error while loadeing ecoloader datas");

            Scheduler scheduler = new Scheduler(loaderData);
            scheduler.Start();

            Console.ReadLine();
            scheduler.Stop();
        }
    }
}
