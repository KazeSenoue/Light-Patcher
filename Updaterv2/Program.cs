using System;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updaterv2
{
    class Program
    {
        static string[] arguments = Environment.GetCommandLineArgs();
        static string pso2path = arguments[1];

        static string baseURL = @"http://download.pso2.jp/patch_prod/patches/";

        static bool CompareMD5(string filename, string serverMD5)
        {
            using (var md5 = MD5.Create())
            {
                try
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
                catch
                {
                    return true;
                }
            }
        }

        static bool DownloadFile(string file, int i, int lineCount)
        {
            using (var client = new WebClient())
            {
                Console.Write("\r| Downloading: {0} | Current: {1} | Total: {2}", file[0], i, lineCount);
                try
                {
                    client.Headers.Add("user-agent", "AQUA_HTTP");
                    client.DownloadFile(baseURL + file, pso2path + file);
                    return true;
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    //Ignores 404s because Sega sucks balls at VCing
                    return true;
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("OK!");
                    return true;
                }
            }
        }

        static void StartUpdate()
        {
            string[] files = new string[] { "patchlist.txt", "launcherlist.txt", "version.ver" };
            Directory.CreateDirectory("temp");

            //Downloads info files
            foreach (string file in files)
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

            //Reads patchlist.txt
            using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            {
                var lineCount = File.ReadLines(@"temp\patchlist.txt").Count();

                var i = 0;

                //Downloads every file on patchlist.txt to PSO2 folder
                while (true)
                {
                    i++;
                    string line = stream.ReadLine();

                    if (line != null)
                    {
                        string[] file = line.Split(null);
                        try
                        {
                            if (CompareMD5(file[0], file[2]) == true)
                            {
                                try
                                {
                                    DownloadFile(file[0], i, lineCount);
                                }
                                catch { }
                            }
                        }
                        catch
                        {
                            DownloadFile(file[0], i, lineCount);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var destination = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SEGA\PHANTASYSTARONLINE2\version.ver";
            File.Copy(@"temp\version.ver", destination, true);
            Directory.Delete("temp", true);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("|----------------------------------------------------------------------------|");

            StartUpdate();
            Console.ReadLine();
        }
    }
}