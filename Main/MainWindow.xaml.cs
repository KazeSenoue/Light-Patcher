using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using SharpCompress.Archives;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SharpCompress.Readers;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Properties.Settings _Settings = Properties.Settings.Default;
        public static MainWindow Main;

        public static void UpdateProgressBar(double value)
        {
            Main.Dispatcher.Invoke(() =>
            {
                Main.progress_bar.Value = value;
            });
        }

        public static void UpdateLabel(string content)
        {
            Main.Dispatcher.Invoke(() =>
            {
                Main.pb_label.Content = content;
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            Main = this;

            if (_Settings.Pso2Path == "")
            {
                MessageBox.Show("Your PSO2 path is empty or invalid. Please select your \"pso2_bin\" folder.");

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\Users";
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok || dialog.FileName.EndsWith("pso2_bin"))
                {
                    _Settings.Pso2Path = dialog.FileName;
                    _Settings.Save();
                    MessageBox.Show("PSO2 path saved successfully.");
                }
            }

            if (!File.Exists("cache.json"))
            {
                MessageBox.Show("It seems this is your first time running Light Patcher. In order for it to work, " +
                                "it needs to cache your PSO2 install, a process that might take a few minutes depending on your install. Please click OK to begin the caching process.");

                //Task.Run(async () => { await Cache.BuildCache(_Settings.Pso2Path); });
            }
        }

        private void launch_button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(_Settings.Pso2Path + "/pso2.exe"))
            {
                var info = new ProcessStartInfo(_Settings.Pso2Path + "/pso2.exe");
                info.EnvironmentVariables.Add("-pso2", "+0x01e3f1e9");
                info.Arguments = "+0x33aca2b9";
                info.UseShellExecute = false;

                var process = new Process();
                process.StartInfo = info;
                process.Start();
            }
        }

        public async Task<Dictionary<string, Dictionary<string, string>[]>> GetDictAsync(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = await client.GetStringAsync(uri);
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>[]>>(content);
            }
        }

        private async void enpatch_button_Click(object sender, RoutedEventArgs e)
        {
            pb_label.Content = "Installing english patch...";

            string url = "https://pitchblack.arghlex.net/pso2/?sort=modtime&order=desc&json";
            string baseUrl = "https://pso2.arghlex.net/pso2/";

            var json = await GetDictAsync(url);
            var file = json["files"][0]["name"];

            string fileUrl = baseUrl + file;
            Directory.CreateDirectory("Temp");
            //await GetFileAsync(fileUrl, Path.Combine(Directory.GetCurrentDirectory(), "Temp"));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var file = File.ReadAllText(@"C:\Users\Rodrigo\Documents\SEGA\PHANTASYSTARONLINE2\user.pso2");
            var regex = @"\s*(.*)\s=\s([^{,]*),?$";
        }
    }
}
