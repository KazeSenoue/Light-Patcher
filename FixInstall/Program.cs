using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShellProgress;

namespace FixInstall
{
    class Program
    {
        static string[] arguments = Environment.GetCommandLineArgs();
        static string pso2path = arguments[1];
        static string baseURL = @"http://download.pso2.jp/patch_prod/patches_old/";

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

        static bool DownloadFile(string file)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    client.DownloadFile(baseURL + file, pso2path + file.Replace(".pat", ""));
                    Console.WriteLine("File: {0}");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        static void Main(string[] args)
        {
            List<string> missingFiles = new List<string>();

            //Downloads required files
            string[] files = new string[] { "patchlist.txt", "launcherlist.txt" };
            Directory.CreateDirectory("temp");

            //Console.WriteLine("|------------------------------------------------------------------------------------------------|");
            Console.WriteLine("| Downloading necessary files...");
            using (var client = new WebClient())
            {
                foreach (var file in files)
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");

                    try
                    {
                        client.DownloadFile(baseURL + file, @"temp\" + file);
                    }
                    catch (Exception e)
                    {
                        //Ignoring Sega's ridiculous 404s because they don't know how to version control
                    }
                }
            }

            //Reads patchlist.txt
            using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            {
                var lineCount = File.ReadLines(@"temp\patchlist.txt").Count();
                var i = 0;

                var maxValue = lineCount;
                var factory = new ProgressBarFactory();
                var progress = factory.CreateInstance(maxValue);

                //Loops through patchlist.txt and writes missing files to update.txt
                while (true)
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        i++;
                        var info = line.Split(null);
                        var fileName = info[0];
                        var serverMD5 = info[2];
                        string currentFile = i.ToString("D5");

                        try
                        {
                            progress.Update(i);
                            Console.Write("\r| Current File: {0} | Total: {1} ", currentFile, lineCount);

                            if (CompareMD5(fileName, serverMD5) == true)
                            {
                                missingFiles.Add(fileName);
                            }
                        }
                        catch (FileNotFoundException e)
                        {
                            missingFiles.Add(fileName);

                        }
                    }
                    else
                    {
                        progress.Complete();
                        break;
                    }
                }

                //Writes missingfiles.txt and empty downloadedfiles.txt
                File.WriteAllLines(@"temp\missingfiles.txt", missingFiles);
                using (StreamWriter file = new StreamWriter(@"temp\downloadedfiles.txt"))
                {
                    file.WriteLine(" ");
                }
            }

            //Downloads every file on missingfiles.txt
            using (StreamReader stream = new StreamReader(@"temp\missingfiles.txt"))
            {
                var lineCount = File.ReadLines(@"temp\missingfiles.txt").Count();
                var downloadedFiles = File.ReadLines(@"temp\downloadedfiles.txt").Count();
                var i = 0;

                var maxValue = lineCount - downloadedFiles;
                var factory = new ProgressBarFactory();
                var progress = factory.CreateInstance(maxValue);

                while (true)
                {
                    var line = stream.ReadLine();
                    if (line != null)
                    {
                        if (!File.ReadAllText(@"temp\downloadedfiles.txt").Contains(line))
                        {
                            i++;

                            string currentFile = i.ToString("D5");

                            progress.Update(i);
                            Console.Write("\r| Current File: {0} | Total: {1} ", currentFile, lineCount - downloadedFiles);

                            if (DownloadFile(line) == true)
                            {
                                using (StreamWriter file = new StreamWriter(@"temp\downloadedfiles.txt"))
                                {
                                    file.WriteLine(line, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Directory.Delete("temp", true);
            Console.Write("| Process finished. You can launch the game now.");
            Console.ReadLine();
        }
    }
}
