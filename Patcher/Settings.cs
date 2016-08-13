using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher
{
    class Settings
    {
        [JsonRequired]
        [JsonProperty("PSO2")]
        public string PSO2 { get; set; }

        //Loads settings
        //Settings settings = new Settings();
        //Settings obj = settings.ReturnSettings();

        public Settings ReturnSettings()
        {
            string settingsFilePath = "settings.json";

            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine("Settings file doesn't exist.");
                throw new Settings.MissingFileException();
            }

            Settings obj = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
            return obj;
        }

        public class MissingFileException: Exception
        {
            public MissingFileException()
            {
            }

        }
    }
}
