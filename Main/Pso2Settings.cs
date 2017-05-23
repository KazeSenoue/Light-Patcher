using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;

namespace Main
{
    class Pso2Settings
    {
        public class Setting
        {
            public string setting;
            public string value;

            public Setting(string setting, string value)
            {
                this.setting = setting;
                this.value = value;
            }
        }

        public static string path = @"C:\user.pso2";
        public static string file = File.ReadAllText(path); //@"C:\Users\Rodrigo\Documents\SEGA\PHANTASYSTARONLINE2\user.pso2"

        public static List<Setting> GetSetting(string setting)
        {
            Regex regex = new Regex($@"({setting}*)\s=\s([^{{,]*),", RegexOptions.Singleline);
            var matches = regex.Matches(file).Cast<Match>().Select(match => new Setting(match.Groups[1].ToString(), match.Groups[2].ToString())).ToList();

            return matches;
        }

        public static void ChangeSetting(string setting, string value, int offset)
        {
            Regex regex = new Regex($@"({setting}*)\s=\s([^{{,]*),", RegexOptions.Singleline);
            var newFile = file.Replace(regex.Matches(file)[offset].Groups[0].ToString(), $"{regex.Matches(file)[offset].Groups[1].ToString()} = {value},");
            File.WriteAllText(path, newFile);
        }
    }
}
