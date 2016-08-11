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
            string patchURL = GetPatchURL();
            string filename = Path.GetFileName(patchURL);
            string directory = "patches";

            Console.WriteLine("Downloading patch...");
            client.DownloadFile(patchURL, filename);

            Console.WriteLine("Extracting patch...");
            ExtractPatch(filename);
        }

        public void ExtractPatch(string filename)
        {
            string directory = "patch";
            string command = string.Format("/C 7z.exe e {0} -o{1}", filename, directory);

            Directory.CreateDirectory(directory);
            Process.Start("cmd.exe", command);
        }
    }
}