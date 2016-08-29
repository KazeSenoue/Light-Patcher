using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishPatchInstaller
{
    class Patcher
    {
        WebClient client = new WebClient();
        string baseURL = "http://108.61.203.33/freedom/patches/enpatch.txt";

        public string GetPatchURL()
        {
            //Gets patch url from var baseURL
            Stream stream = client.OpenRead(baseURL);
            StreamReader reader = new StreamReader(stream);

            return reader.ReadLine();
        }

        public void DownloadPatch()
        {
            //Downloads patch using getPatchURL()
            var patchURL = GetPatchURL();
            var filename = Path.GetFileName(patchURL);

            Console.WriteLine("######################################");
            Console.WriteLine("");
            Console.WriteLine("Downloading english patch...\n");
            client.DownloadFile(patchURL, filename);

            Console.WriteLine("Extracting patch...\n");
            ExtractPatch(filename);
        }

        public void ExtractPatch(string filename)
        {
            var directory = "patch";
            var command = string.Format("/C 7z.exe e {0} -o{1} -aoa", filename, directory);

            Directory.CreateDirectory(directory);
            var process = Process.Start("cmd.exe", command);
            process.WaitForExit();

            Console.WriteLine("Extracted!\n");
            InstallPatch();
        }

        public void InstallPatch()
        {
            var arguments = Environment.GetCommandLineArgs();
            var win32dir = arguments[1] + "\\data\\win32\\";

            if (arguments.Length == 1)
            {
                Console.WriteLine("No arguments passed. Press any buttons to exit.");
                Console.ReadKey();
            }
            else
            {
                var files = Directory.GetFiles("patch");

                foreach (var file in files)
                {
                    Console.WriteLine("Installing english patch...\n");

                    var filename = file.Replace("patch\\", "");
                    var FileToReplace = win32dir + filename;

                    try
                    {
                        File.Replace(file, FileToReplace, FileToReplace + ".bak");
                    }
                    catch (System.IO.IOException)
                    {
                        Console.WriteLine("Your patcher is not in the same drive as your PSO2 installation.");
                        Console.WriteLine("Press any button to exit...");
                        Console.ReadKey();
                        return;
                    }


                }
                Console.WriteLine("English patch successfully installed!");
                Console.ReadKey();
            }
        }
    }
}