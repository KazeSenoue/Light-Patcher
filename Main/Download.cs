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
            if (url.Contains(@"win32"))
                url = url + ".pat";

            using (var client = new WebClient())
            {
                client.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) { client_DownloadFileCompleted(sender, e, location, i, total); };
                client.Headers.Add("User-Agent", "AQUA_HTTP");
                await client.DownloadFileTaskAsync(new Uri(url), location);
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
                await GetFileAsync(Path.Combine(baseURL, file.File + ".pat"), Path.Combine(_Settings.Pso2Path, file.File), i, list.Count);
            }
        }
    }
}
