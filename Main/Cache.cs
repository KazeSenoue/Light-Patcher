using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    class Cache
    {
        [JsonRequired]
        [JsonProperty("file")]
        public string File { get; set; }

        [JsonRequired]
        [JsonProperty("MD5")]
        public string MD5 { get; set; }

        [JsonRequired]
        [JsonProperty("lastModified")]
        public long? LastModified { get; set; }

        public Cache(string File, string MD5 = null, long? LastModified = null)
        {
            this.File = File;
            this.MD5 = MD5;
            this.LastModified = LastModified;
        }

        public static async Task<string> CalculateHash(string file, System.Security.Cryptography.MD5 md5)
        {
            Task<string> MD5 = Task.Run(() =>
            {
                {
                    using (var stream = new BufferedStream(System.IO.File.OpenRead(file), 1200000))
                    {
                        var hash = md5.ComputeHash(stream);
                        var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                        return fileMD5;
                    }
                };
            });

            return await MD5;
        }

        public static void UpdateCache(string file, string MD5, long lastModified)
        {
            var cache = JsonConvert.DeserializeObject<List<Cache>>(System.IO.File.ReadAllText("cache.json"));
            int index = cache.FindIndex(f => f.File == file);

            if (index >= 0)
            {
                cache[index].MD5 = MD5;
                cache[index].LastModified = lastModified;
            }
            else
            {
                cache.Add(new Cache(file, MD5, lastModified));
            }
        }

        public static List<Cache> ReadCache(string directory)
        {
            return JsonConvert.DeserializeObject<List<Cache>>(System.IO.File.ReadAllText(directory));
        }

        public static async Task BuildCache(string directory)
        {
            var fileList = Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories).Select(f => new FileInfo(f)).ToList();
            List<Cache> cacheFileList = new List<Cache>();
            int i = 0;

            foreach (var file in fileList)
            {
                i++;

                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    cacheFileList.Add(new Cache(file.FullName.Substring(directory.Length + 1), await CalculateHash(file.FullName, md5), new DateTimeOffset(file.LastWriteTimeUtc).ToUnixTimeMilliseconds()));
                }

                MainWindow.UpdateLabel($"{i} / {fileList.Count()} - {(double)(100 * i) / fileList.Count():F3}%");
                MainWindow.UpdateProgressBar((100 * i) / fileList.Count());
            }

            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "cache.json"), JsonConvert.SerializeObject(cacheFileList));
        }

        public static List<Cache> ReturnMissingFiles(string cache, string directory)
        {
            List<Cache> cacheFileList = JsonConvert.DeserializeObject<List<Cache>>(cache);
            var fileList = Directory.GetFiles(directory)
                .Select(f => new FileInfo(f))
                .ToDictionary(x => x.Name, x => new DateTimeOffset(x.LastWriteTimeUtc).ToUnixTimeMilliseconds());

            var returnList = new List<Cache>();

            foreach (var file in cacheFileList)
            {
                if (fileList.Keys.Contains(file.File))
                {
                    if (file.LastModified != fileList[file.File])
                        returnList.Add(file);
                }
                else
                {
                    returnList.Add(file);
                }
            }

            return returnList;
        }
    }
}
