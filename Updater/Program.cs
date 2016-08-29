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

        static void GetInfo(string url)
        {
            //Downloads new files
            Directory.CreateDirectory("temp");
            foreach (var file in files)
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");

                    if (url != oldBaseURL || file != "launcherlist.txt")
                    {
                        Console.WriteLine("| Downloading: {0}", file);
                        client.DownloadFile(url + file, "temp\\" + file);
                    }

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

        static bool CompareMD5(string filename, string serverMD5, string pso2path)
        {
            
            var path = pso2path + @"\";

            string[] blacklist = new string[] { ".exe", ".des", ".ver", ".ini", ".txt" };

            if (!blacklist.Contains(filename))
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(File.ReadAllBytes(path + filename.Replace(".pat", "")));
                    var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                    //Console.WriteLine(filename, fileMD5);

                    if (fileMD5 != serverMD5)
                    {
                        //using (var client2 = new WebClient())
                        //{
                        //    client2.Headers.Add("user-agent", "AQUA_HTTP");
                        //    client2.DownloadFile(dlURL + filename + ".pat", "temp\\" + filename);

                        //    File.Copy(@"temp\" + filename, path + filename, true);
                        //    return true;
                        //}

                        StreamWriter file = new StreamWriter(@"temp\update.txt", true);
                        file.WriteLine(filename);

                        file.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        static bool Downloader(string pso2path)
        {
            using (StreamReader stream = new StreamReader(@"temp\update.txt"))
            {
                while (true)
                {
                    string line = stream.ReadLine();

                    using (var client = new WebClient())
                    {
                        client.Headers.Add("user-agent", "AQUA_HTTP");
                        Console.WriteLine("| Downloading: {0}", line);
                        client.DownloadFile(baseURL + line, pso2path + @"\" + line);

                        return true;
                    }
                }
            }
        }

        static void CheckFiles(string pso2path)
        {
            //Reads patchlist.txt
            using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            {
                var lineCount = File.ReadLines(@"temp\patchlist.txt").Count();

                var i = 0;
                var updated = 0;

                Console.WriteLine("| Updating...");

                //Loops through patchlist.txt and writes missing files to update.txt
                while (true)
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        var info = line.Split(null);
                        var filename = info[0];
                        var newMD5 = info[2];

                        try
                        {
                            if (CompareMD5(filename, newMD5, pso2path) == true)
                            {
                                updated++;
                            }
                        }
                        catch (FileNotFoundException e)
                        {
                            StreamWriter file = new StreamWriter(@"temp\update.txt", true);
                            file.WriteLine(filename);

                            file.Close();
                        }

                        i++;
                        Console.Write("\r| File: {0} | Checked: {1} | Total: {2}", filename, i, lineCount);

                    }
                    else
                    {
                        break;
                    }
                }

                //Downloads every file on update.txt and puts them on the appropriate folder
            }
        }

        static void MissingFilesCheck(string pso2path)
        {
            //Reads patchlist.txt
            using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            {
                var lineCount = File.ReadLines(@"temp\patchlist.txt").Count();

                Console.WriteLine("| Checking...");

                foreach (string file in files)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("|--------------------------------------------------|");

            var arguments = Environment.GetCommandLineArgs();
            
            switch (arguments[2])
            {
                case "-update":
                    GetInfo(baseURL);
                    CheckFiles(arguments[1]);
                    Downloader(arguments[1]);
                    break;

                case "-missingFilesCheck":
                    GetInfo(oldBaseURL);
                    //CheckFiles(arguments[1]);
                    Downloader(arguments[1]);
                    break;
            }

            Console.ReadLine();
        }
    }
}
