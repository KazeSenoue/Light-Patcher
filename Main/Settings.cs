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
        public string pso2_path { get; set; }

        public Settings ReturnSettings()
        {
            string settings_file_path = "settings.json";

            if (!File.Exists(settings_file_path))
            {
                Settings _data = new Settings();
                _data.pso2_path = "";

                string json = JsonConvert.SerializeObject(_data);
                File.WriteAllText(settings_file_path, json);

                return _data;
            }

            Settings obj = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settings_file_path));
            return obj;
        }

    }
}
