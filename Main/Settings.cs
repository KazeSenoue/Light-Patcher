using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Main
{
    class Settings
    {
        [JsonRequired]
        [JsonProperty("pso2_path")]
        public string pso2Path { get; set; }

        public Settings ReturnSettings()
        {
            string settingsFilePath = "settings.json";

            if (!File.Exists(settingsFilePath))
            {
                Settings _data = new Settings();
                _data.pso2Path = "";

                string json = JsonConvert.SerializeObject(_data);
                File.WriteAllText(settingsFilePath, json);

                return _data;
            }

            Settings obj = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
            return obj;
        }

    }
}
