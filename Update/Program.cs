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
        static int threadCount = 4;
        static string pso2path = arguments[1];

        static string baseURL = @"http://download.pso2.jp/patch_prod/patches/";

        static bool DownloadFile(string file, int index, int total, string temppath)
        {
            bool success = false;
            float percentage = (index / total) * 100;

            Console.WriteLine("| Downloading: {0} ({1} / {2}) | Progress: {3}%", file, index, total, percentage);

            
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    client.DownloadFile(baseURL + file, temppath + file.Replace(".pat", ""));
                    success = true;
                }
            }
            catch (WebException ex)
            {
                if ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    //Ignores 404s because Sega sucks balls at VCing
                    success = true;
                }
                else if (ex.Status == WebExceptionStatus.Timeout)
                {
                    //???
                    Console.WriteLine("Timeout.");
                }
                else if ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Forbidden.");
                }
                else
                    Console.WriteLine(ex.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        static void DownloadUpdatedFiles(List<string> missingFiles, string temppath)
        {
            // Downloads every file on files list
            int atomicIndex = 0;

            var threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                var t = new Thread(() =>
                {
                    while (true)
                    {
                        var index = Interlocked.Increment(ref atomicIndex);
                        if (index >= missingFiles.Count)
                            break;

                        try
                        {
                            DownloadFile(missingFiles[index] + ".pat", index, missingFiles.Count, temppath);
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
            //Creates a folder on temp
            string temppath = Path.GetTempPath();
            Directory.CreateDirectory(temppath + @"LightPatcher\pso2_bin\data\win32\script");
            temppath = temppath + @"LightPatcher\pso2_bin\";

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
                        client.DownloadFile(baseURL + file, temppath + Path.GetFileName(file));
                        client.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message, e.ToString());
                    }
                }
            }

            //Reads patchlist.txt to list
            var segaFiles = new List<string>();
            foreach (string line in File.ReadLines(temppath + @"patchlist.txt").ToList())
            {
                var newLine = line.Split(null);
                segaFiles.Add(newLine[0].Replace(".pat", ""));
            }

            //Scans temp folder for pso2 files and adds missing ones to a list
            IEnumerable<string> files = Directory.EnumerateFiles(temppath, "*.*", SearchOption.AllDirectories);
            var fixedFiles = new List<string>();

            foreach (string file in files)
            {
                fixedFiles.Add(file.Replace(temppath, "").Replace(@"\", @"/"));
            }

            var missingFiles = segaFiles.Except(fixedFiles).ToList();
            
            //Downloads missing files
            for (int retries = 0; retries < 10; retries++) {
                try
                {
                    DownloadUpdatedFiles(missingFiles, temppath);
                    break;
                }
                catch (Exception)
                { 
                    if (retries < 5)
                    {
                        Console.WriteLine("| Something went wrong. Retrying...");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("| Failed to update.");
                    }
                }
            }

            //Moves files on temp folder to pso2_bin
            try
            {
                var targetPath = pso2path;
                var sourcePath = temppath + @"data\win32\";

                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] newFiles = System.IO.Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in newFiles)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        var fileName = System.IO.Path.GetFileName(s);
                        var destFile = System.IO.Path.Combine(targetPath, fileName);
                        Console.Write("\r| Moving: {0}                         ", fileName);
                        System.IO.File.Copy(s, destFile, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Update successful.");
            Console.ReadLine();
        }
    }
}
