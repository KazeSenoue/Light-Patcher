using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher
{
    class Settings
    {
        [JsonRequired]
        [JsonProperty("ProxyURL")]
        public string ProxyURL { get; set; }

        [JsonRequired]
        [JsonProperty("PSO2")]
        public string PSO2 { get; set; }

        //Loads settings
        //Settings settings = new Settings();
        //Settings obj = settings.ReturnSettings();

        public Settings ReturnSettings()
        {
            string settingsFilePath = "settisadasdng.json";

            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine("Settings file doesn't exist.");
                throw new Settings.MissingFileException();
            }

            Settings obj = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
            return obj;
        }

        public void SavePSO2Dir()
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();

            //Prompts user to select pso2_dir
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            //If the folder is "pso2_bin", saves it to settings.json. If not, tells the user they fucked up.
            var selectedDir = dialog.FileName;
            if (selectedDir.EndsWith("pso2_bin") && File.Exists("settings.json"))
            {
                settings.PSO2 = selectedDir + @"\";
                string output = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("settings.json", output);
                MessageBox.Show(String.Format("Saved!\nSelected folder: {0}", settings.PSO2));
            }
            else
            {
                MessageBoxResult retry = MessageBox.Show(String.Format("{0} is not a valid PSO2 folder. Retry?", selectedDir), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (retry == MessageBoxResult.Yes)
                {
                    SavePSO2Dir();
                }
                else
                {
                    MessageBox.Show("Alrighty. Bye bye!");
                    Application.Current.Shutdown();
                }
            }
        }

public class MissingFileException: Exception
        {
            public MissingFileException()
            {
            }

        }
    }
}
