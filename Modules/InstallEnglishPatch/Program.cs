using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace EnglishPatchInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseURL = "http://108.61.203.33/freedom/patches/enpatch.txt";
            string pso2path = Environment.GetCommandLineArgs()[1];

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(baseURL);
            StreamReader reader = new StreamReader(stream);

            string patchURL = reader.ReadLine();
            
            if (!File.Exists(@"7za.exe"))
            {
                Console.WriteLine("7za.exe not found. Downloading...");
                client.DownloadFile("http://kazesenoue.moe/pso2/7za.exe", "7za.exe");
            }

            //Creates a folder on temp
            string temppath = Path.GetTempPath();
            Directory.CreateDirectory(temppath + @"LightPatcherInfo\");
            temppath = temppath + @"LightPatcherInfo\";

            //Downloads patch
            var filename = Path.GetFileName(patchURL);
            Console.WriteLine("Downloading english patch...");
            client.DownloadFile(patchURL, temppath + filename);

            //Extracts patch to pso2_bin
            Console.WriteLine("Extracting...");
            var directory = pso2path + @"data\win32\";
            var command = string.Format("/C 7z.exe e {0} -o{1} -aoa", filename, directory);
            var process = Process.Start("cmd.exe", command);
            process.WaitForExit();

            Console.WriteLine("Successful!");
            Console.ReadLine();
        }
    }
}