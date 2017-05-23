using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using SharpCompress;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
using SharpCompress.Archives;
using Newtonsoft.Json;
using SharpCompress.Archives.Zip;

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

        public async static Task GetFileAsync(string url, string location, int i, int total)
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

        public static async Task GetEnglishPatch(string URL)
        {
            var file = Path.GetFileName(URL);

            // Downloads Rar file
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(URL), Path.Combine("Temp/", file));
            }

            await Task.Delay(2000);

            // Unzips Rar file
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var archive = ZipArchive.Open(Path.Combine("Temp/", file)))
            {
                var json = JsonConvert.DeserializeObject<List<Cache>>(File.ReadAllText("patch_cache.json"));
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(Path.Combine(_Settings.Pso2Path, "data/win32"), new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });

                    var hash = await Cache.CalculateHash(Path.Combine(_Settings.Pso2Path, "data/win32", entry.Key), md5);
                    var lastModified = new DateTimeOffset(new FileInfo(Path.Combine(_Settings.Pso2Path, "data/win32", entry.Key)).LastWriteTimeUtc).ToUnixTimeMilliseconds();

                    if (!json.Any(x => x.MD5 == hash))
                        json.Add(new Cache(entry.Key, hash, lastModified));
                }
                File.WriteAllText("patch_cache.json", JsonConvert.SerializeObject(json));
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
                    try
                    {
                        var baseURLOld = "http://download.pso2.jp/patch_prod/patches_old/";
                        await GetFileAsync(Path.Combine(baseURLOld, file.File + ".pat"), Path.Combine(_Settings.Pso2Path, file.File), i, list.Count);
                    }
                    catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            MainWindow.EnableButtons();
            MainWindow.UpdateLabel("Successful!");
        }
    }
}
