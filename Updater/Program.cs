using System;
using System.Security.Cryptography;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static string baseURL = "http://download.pso2.jp/patch_prod/patches/";
        static string dlURL = "http://download.pso2.jp/patch_prod/patches/data/win32/";
        static string oldBaseURL = "http://download.pso2.jp/patch_prod/patches_old/";

        static string[] files = new string[] {"patchlist.txt", "launcherlist.txt", "version.ver"};

        static void GetInfo()
        {
            //Downloads new files
            Directory.CreateDirectory("temp");
            foreach (var file in files)
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    Console.WriteLine("| Downloading: {0}", file);
                    client.DownloadFile(baseURL + file, "temp\\" + file);
                    client.Dispose();
                }
            }
            
            Console.WriteLine("| Done!");
        }

        static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        static void CompareMD5(string filename, string serverMD5)
        {
            var path = @"G:\PHANTASYSTARONLINE2\pso2_bin2\data\win32\";
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(File.ReadAllBytes(path + filename));
                var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                if (fileMD5 != serverMD5)
                {
                    using (var client2 = new WebClient())
                    {
                        Console.WriteLine("| Downloading: {0}", filename);
                        client2.Headers.Add("user-agent", "AQUA_HTTP");
                        client2.DownloadFile(dlURL + filename + ".pat", "temp\\" + filename);
                    }
                }
                else
                {
                    Console.WriteLine("Skipping {0}", filename);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("|--------------------------------------------------|");
            GetInfo();

            //Reads patchlist.txt
            using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            {
                while (true)
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        var info = line.Split(null);
                        var filename = Path.GetFileName(info[0]).Replace(".pat", "");
                        var newMD5 = info[2];

                        CompareMD5(filename, newMD5);
                    }
                    else
                    {
                        break;
                    }
                }
                
                
            }

            Console.ReadLine();
        }
    }
}
