using System;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Update
{
    class Program
    {
        static string[] arguments = Environment.GetCommandLineArgs();
        static int threadCount = Int32.Parse(arguments[2]);
        static string pso2path = arguments[1];

        static string baseURL = @"http://download.pso2.jp/patch_prod/patches/";

        static bool DownloadFile(string file, int index, int total)
        {
            bool success = false;

            Console.WriteLine("| Downloading: {0} ({1} / {2})", file, index, total);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    client.DownloadFile(baseURL + file, pso2path + file);
                    success = true;
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    success = true;
                    //Ignores 404s because Sega sucks balls at VCing
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.OK)
                {
                    success = true;
                    Console.WriteLine("OK!");
                }
            }

            if (success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("|----------------------------------------------------------------------------|");

            string[] infoFiles = new string[] { "patchlist.txt", "launcherlist.txt", "version.ver" };
            Directory.CreateDirectory("temp");

            //Downloads information files
            foreach (string file in infoFiles)
            {
                using (var client = new WebClient())
                {
                    Console.WriteLine("| Downloading: {0}", file);

                    try
                    {
                        client.Headers.Add("user-agent", "AQUA_HTTP");
                        client.DownloadFile(baseURL + file, @"temp\" + Path.GetFileName(file));
                        client.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message, e.ToString());
                    }
                }
            }

            //Reads patchlist.txt to list
            var files = new List<string>();
            foreach (string line in File.ReadLines(@"temp\patchlist.txt").ToList())
            {
                var newLine = line.Split(null);
                files.Add(newLine[0]);
            }

            int atomicIndex = 0;

            var threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                var t = new Thread(() =>
                {
                    while (true)
                    {
                        var index = Interlocked.Increment(ref atomicIndex);
                        if (index >= files.Count)
                            break;

                        DownloadFile(files[index], index, files.Count);
                    }
                });

                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads)
                t.Join();

            Console.WriteLine("Update successful.");
            Console.ReadLine();
        }
    }
}
