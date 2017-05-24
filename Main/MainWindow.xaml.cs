using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        public static void EnableButtons()
        {
            Main.Dispatcher.Invoke(() =>
            {
                Main.launch_button.IsEnabled = true;
                Main.enpatch_button.IsEnabled = true;
            });
        }

        public static void DisableButtons()
        {
            Main.Dispatcher.Invoke(() =>
            {
                Main.launch_button.IsEnabled = false;
                Main.enpatch_button.IsEnabled = false;
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

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (dialog.FileName.EndsWith("pso2_bin"))
                    {
                        _Settings.Pso2Path = dialog.FileName;
                        _Settings.Save();
                        MessageBox.Show("PSO2 path saved successfully.");
                    }
                    else
                    {
                        var choice = MessageBox.Show($"Light Patcher will install PSO2 on the following folder: {dialog.FileName + @"\PHANTASYSTARONLINE2"}. Is that correct?", "Light Patcher", MessageBoxButton.YesNo);
                        if (choice == MessageBoxResult.Yes)
                        {
                            Directory.CreateDirectory(dialog.FileName + @"\PHANTASYSTARONLINE2\pso2_bin");
                            _Settings.Pso2Path = dialog.FileName + @"\PHANTASYSTARONLINE2\pso2_bin";
                            _Settings.Save();

                            MessageBox.Show("Downloading the necessary files. Please wait...");
                            Task.Run(async () => await Download.GetPatchfiles());
                        }
                        else
                        {
                            MessageBox.Show("Process canceled.", "Light Patcher");
                            Environment.Exit(0);
                        }
                    }
                }
            }

            if (!File.Exists("cache.json"))
            {
                MessageBox.Show("It seems this is your first time running Light Patcher. In order for it to work, " +
                                "it needs to cache your PSO2 install, a process that might take a few minutes depending on your install. Please click OK to begin the caching process.");

                Task.Run(async () => { await Cache.BuildCache(_Settings.Pso2Path); });
            }

            // Checks and downloads missing / corrupt files
            var files = Cache.ReturnMissingFiles();
            var downloadList = files["missingFiles"].Union(files["corruptFiles"]).ToList();

            DisableButtons();
            Download.GetFilesAsync(downloadList);
        }

        private void Launch_button_Click(object sender, RoutedEventArgs e)
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
                Application.Current.Shutdown();
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

        private async void Enpatch_button_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            if (!File.Exists("patch_cache.json"))
            {
                File.WriteAllText("patch_cache.json", "[]");
            }
            pb_label.Content = "Installing english patch...";

            string url = "https://pso2.arghlex.net/pso2/?sort=modtime&order=desc&json";
            string baseUrl = "https://pso2.arghlex.net/pso2/";

            var fileUrl = baseUrl + (await GetDictAsync(url))["files"][0]["name"];

            Directory.CreateDirectory("Temp");
            await Download.GetEnglishPatch(fileUrl);
            Directory.Delete("Temp", true);
            pb_label.Content = "Successful!";
            EnableButtons();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var file = File.ReadAllText(@"C:\Users\Rodrigo\Documents\SEGA\PHANTASYSTARONLINE2\user.pso2");
            var regex = @"\s*(.*)\s=\s([^{,]*),?$";
        }
    }
}
