using System;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FixFiles
{
    class Program
    {
        static string pso2path = Environment.GetCommandLineArgs()[1];
        static string patchURL = @"http://download.pso2.jp/patch_prod/patches/";
        static string temppath = Path.GetTempPath() + @"LightPatcherInfo\";

        static bool CompareMD5(string filename, string serverMD5)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(File.ReadAllBytes(pso2path + filename.Replace(".pat", "")));
                var fileMD5 = string.Concat(Array.ConvertAll(hash, x => x.ToString("X2")));

                if (fileMD5 != serverMD5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        static void CheckMissingFiles(Dictionary<string, string> segaFiles, string temppath)
        {
            // Downloads every file on files list
            int atomicIndex = 0;

            var threads = new List<Thread>();

            for (int i = 0; i < 4; i++)
            {
                var t = new Thread(() =>
                {
                    while (true)
                    {
                        var index = Interlocked.Increment(ref atomicIndex);
                        if (index >= segaFiles.Count)
                            break;

                        try
                        {
                            foreach (var file in segaFiles)
                            {
                                if(CompareMD5(file, serverMD5) == true)
                                {

                                }
                            }
                        }
                        catch (WebException ex)
                        {
                            throw new Exception(ex.Message);
                        }

                    }
                });

                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads)
                t.Join();
        }

        static void Main(string[] args)
        {
            //Downloads patchlist.txt
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    client.DownloadFile(patchURL + "patchlist.txt", temppath + "patchlist.txt");
                }
                using (StreamReader stream = new StreamReader(temppath + "patchlist.txt"))
                {
                    //Adds files to a list
                    var segaFiles = new Dictionary<string, string>();
                    foreach (string line in File.ReadLines(temppath + @"patchlist.txt").ToList())
                    {
                        var newLine = line.Split(null);
                        segaFiles.Add(newLine[0].Replace(".pat", ""), newLine[2]);
                    }

                    //Loops through segaFiles and writes missing files to update.txt
                    CheckMissingFiles(segaFiles, temppath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not download patchlist.\nError: {0}", e.Message);
            }
        }
        
    }
}
