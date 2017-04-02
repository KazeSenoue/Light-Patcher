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

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings _Settings = new Settings().ReturnSettings();

        public MainWindow()
        {
            // Beggining of file check
            using (var client = new WebClient())
            {
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");
                
                client.Headers.Add("User-Agent", "AQUA_HTTP");
                client.DownloadFile("http://download.pso2.jp/patch_prod/patches/patchlist.txt", "Temp/patchlist_old.txt");

                client.Headers.Add("User-Agent", "AQUA_HTTP");
                client.DownloadFile("http://download.pso2.jp/patch_prod/patches_old/patchlist.txt", "Temp/patchlist.txt");
            }

            string[] fileList = System.IO.File.ReadAllLines("Temp/patchlist_old.txt").Concat(System.IO.File.ReadAllLines("Temp/patchlist.txt")).ToArray();

            List<Cache> cache = Cache.ReadCache("cache.json");
            List<File> missingFiles = new List<File>();
            foreach (var line in fileList)
            {
                var file = line.Split();

                int index = cache.FindIndex(f => f.File == file[0].Replace(".pat", ""));
                if (index >= 0)
                {
                    if (cache[index].MD5 != file[2])
                    {
                        missingFiles.Add(new File(file[0], file[2]));
                    }
                }
                else
                {
                    missingFiles.Add(new File(file[0], file[2]));
                }
            }

            MessageBox.Show($"Missing files: {missingFiles.Count.ToString()}");
        }

        private void launch_button_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(_Settings.Pso2Path + "/pso2.exe"))
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

        public async Task GetFileAsync(string uri, string location)
        {
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) { client_DownloadFileCompleted(sender, e, uri.ToString()); };

                client.DownloadFileAsync(new Uri(uri), Path.Combine(location, Path.GetFileName(uri)));
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progress_bar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, string uri)
        {
            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(Path.Combine("Temp/", Path.GetFileName(uri))))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(Path.Combine(_Settings.Pso2Path, @"data/win32"), new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }

            Directory.Delete("Temp", true);
            pb_label.Content = "Success!";
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
            await GetFileAsync(fileUrl, Path.Combine(Directory.GetCurrentDirectory(), "Temp"));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var file = System.IO.File.ReadAllText(@"C:\Users\Rodrigo\Documents\SEGA\PHANTASYSTARONLINE2\user.pso2");
            var regex = @"\s*(.*)\s=\s([^{,]*),?$";
        }
    }
}
