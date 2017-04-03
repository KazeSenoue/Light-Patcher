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
        public static MainWindow Main;

        class FileCacheEntryNameComparer : IEqualityComparer<Cache>
        {
            public bool Equals(Cache x, Cache y) => x.File == y.File;
            public int GetHashCode(Cache obj) => obj.File.GetHashCode();
        }

        class FileCacheEntryLastModifiedComparer : IEqualityComparer<Cache>
        {
            public bool Equals(Cache x, Cache y) => x.LastModified == y.LastModified;
            public int GetHashCode(Cache obj) => obj.LastModified.GetHashCode();
        }

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

            if (!System.IO.File.Exists("cache.json"))
            {
                MessageBox.Show("It seems this is your first time running Light Patcher. In order for it to work, " +
                                "it needs to cache your PSO2 install, a process that might take a few minutes depending on your install. Please click OK to begin the caching process.");

                Task.Run(async () => { await Cache.BuildCache(_Settings.Pso2Path); });
            }

            if (System.IO.File.Exists("cache.json"))
            {
                //Beggining of file check
                using (var client = new WebClient())
                {
                    if (!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");

                    client.Headers.Add("User-Agent", "AQUA_HTTP");
                    client.DownloadFile("http://download.pso2.jp/patch_prod/patches/patchlist.txt", "Temp/patchlist_old.txt");

                    client.Headers.Add("User-Agent", "AQUA_HTTP");
                    client.DownloadFile("http://download.pso2.jp/patch_prod/patches_old/patchlist.txt", "Temp/patchlist.txt");
                }

                List<Cache> fileList = System.IO.File.ReadAllLines("Temp/patchlist_old.txt").Concat(System.IO.File.ReadAllLines("Temp/patchlist.txt"))
                    .Select(f => new Cache(Path.GetFileNameWithoutExtension(f.Split()[0]), f.Split()[2]))
                    .ToList();


                List<Cache> localFiles = Directory.GetFiles(_Settings.Pso2Path, "*.*", System.IO.SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .Select(f => new Cache(f.Name, null, new DateTimeOffset(f.LastWriteTimeUtc).ToUnixTimeMilliseconds()))
                    .ToList();

                List<Cache> cache = Cache.ReadCache("cache.json").Select(i => new Cache(i.File, i.MD5)).ToList();
                List<Cache> missingFiles = fileList.Except(cache, new FileCacheEntryNameComparer()).ToList();
                List<Cache> modifiedFiles = localFiles.Except(cache, new FileCacheEntryLastModifiedComparer())
                    .Except(missingFiles, new FileCacheEntryNameComparer())
                    .ToList();

                List<Cache> corruptFiles = modifiedFiles.Where(e =>
                {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    using (var stream = new BufferedStream(System.IO.File.OpenRead(e.File), 12000000))
                    {

                        var hash = md5.ComputeHash(System.IO.File.ReadAllBytes(e.File));
                        var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                        return fileMD5 != e.MD5;
                    }
                }).ToList();

                if (missingFiles.Count() > 0)
                    MessageBox.Show($"Missing files: {missingFiles.Count().ToString()}\nCorrupt files: {corruptFiles.Count().ToString()}");
            }
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
