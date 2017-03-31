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
        public long LastModified { get; set; }

        public Cache(string File, string MD5, long LastModified)
        {
            this.File = File;
            this.MD5 = MD5;
            this.LastModified = LastModified;
        }

        public static string CalculateHash(string file)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(System.IO.File.ReadAllBytes(file));
                var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                return fileMD5;
            }
        }

        public static void BuildCache(string directory)
        {
            var fileList = Directory.GetFiles(directory).Select(f => new FileInfo(f));
            List<Cache> cacheFileList = new List<Cache>();

            foreach (var file in fileList)
            {
                cacheFileList.Add(new Cache(file.Name, CalculateHash(file.FullName), new DateTimeOffset(file.LastWriteTimeUtc).ToUnixTimeMilliseconds()));
            }

            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "cache.json"), JsonConvert.SerializeObject(cacheFileList));
        }

        public static List<Cache> ReturnMissingFiles(string cache, string directory)
        {
            List<Cache> cacheFileList = JsonConvert.DeserializeObject<List<Cache>>(cache);
            var fileList = Directory.GetFiles(directory).Select(f => new FileInfo(f)).ToDictionary(x => x.Name, x => new DateTimeOffset(x.LastWriteTimeUtc).ToUnixTimeMilliseconds());

            var returnList = new List<Cache>();

            foreach (var file in cacheFileList)
            {
                if (fileList.Keys.Contains(file.File))
                {
                    if (file.LastModified < fileList[file.File])
                    {
                        returnList.Add(file);
                    }
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
