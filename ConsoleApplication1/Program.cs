using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //THIS IS FOR CHECKING FOR OLD / MISSING FILES
            //string[] files = Directory.GetFiles("G:\\PHANTASYSTARONLINE2\\pso2_bin\\data\\win32\\");

            //foreach (string file in files)
            //{
            //    Console.WriteLine(Path.GetFileName(file));
            //}

            string arguments = @"-p G:\PHANTASYSTARONLINE2\pso2_bin -update";
            var lul = arguments.Split(new[] { "-p ",  }, StringSplitOptions.None);

            Console.WriteLine(lul[1]);
            Console.ReadLine();
        }
    }
}
