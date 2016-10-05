using System;
using System.Net;
using System.IO;
using ShellProgress;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadClient
{
    class Program
    {
        static string[] arguments = Environment.GetCommandLineArgs();
        static string pso2path = arguments[1];
        static string baseURL = @"http://download.pso2.jp/patch_prod/patches_old/";

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
            //Downloads required files
            string[] files = new string[] { "patchlist.txt", "launcherlist.txt" };
            Directory.CreateDirectory("temp");
            
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

            ////Reads patchlist.txt
            //using (StreamReader stream = new StreamReader(@"temp\patchlist.txt"))
            //{
            //    var lineCount = File.ReadLines(@"temp\patchlist.txt").Count();
            //    var i = 0;

            //    var maxValue = lineCount;
            //    var factory = new ProgressBarFactory();
            //    var progress = factory.CreateInstance(maxValue);
            //}
        }
}
