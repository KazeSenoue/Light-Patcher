using System;
using System.Text;
using System.Threading.Tasks;

namespace EnglishPatchInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Patcher patcher = new Patcher();
            patcher.DownloadPatch();
        }
    }
}