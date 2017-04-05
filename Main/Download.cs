using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;

namespace Main
{
    class Download
    {
        private static Properties.Settings _Settings = Properties.Settings.Default;
        private static async void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, string file, int i, int total)
        {
            MainWindow.UpdateProgressBar((double)(i / 100) * total);
            MainWindow.UpdateLabel($"{i} / {total}");
            await Cache.UpdateCache(_Settings.Pso2Path, file);
        }

        private async static Task GetFileAsync(string url, string location, int i, int total)
        {
            Debug.WriteLine(url);
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) { client_DownloadFileCompleted(sender, e, location, i, total); };
                    client.Headers.Add("User-Agent", "AQUA_HTTP");
                    await client.DownloadFileTaskAsync(new Uri(url), location);
                }
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        public static async Task GetPatchfiles()
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "AQUA_HTTP");
                var launcherFiles = await client.DownloadStringTaskAsync(new Uri("http://download.pso2.jp/patch_prod/patches/launcherlist.txt"));
                
                foreach (var file in launcherFiles.Split('\n'))
                {
                    var newFile = file.Split();

                    client.Headers.Add("User-Agent", "AQUA_HTTP");
                    await client.DownloadFileTaskAsync(new Uri(newFile[0]), Path.Combine(_Settings.Pso2Path, newFile[0].Replace(".pat", "")));
                    await Cache.UpdateCache(_Settings.Pso2Path, newFile[0].Replace(".pat", ""));
                }
            }
        }

        public async static void GetFilesAsync(List<Cache> list)
        {
            var baseURL = "http://download.pso2.jp/patch_prod/patches/";
            MainWindow.UpdateLabel($"1 / {list.Count}");

            var i = 0;
            foreach (var file in list)
            {
                i++;
                try
                {
                    await GetFileAsync(Path.Combine(baseURL, file.File + ".pat"), Path.Combine(_Settings.Pso2Path, file.File), i, list.Count);
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    var baseURLOld = "http://download.pso2.jp/patch_prod/patches_old/";
                    await GetFileAsync(Path.Combine(baseURLOld, file.File + ".pat"), Path.Combine(_Settings.Pso2Path, file.File), i, list.Count);
                }
            }

            MainWindow.EnableButtons();
            MainWindow.UpdateLabel("Successful!");
        }
    }
}
