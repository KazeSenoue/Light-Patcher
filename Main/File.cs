using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    class File
    {
        public string Name { get; set; }
        public string MD5 { get; set; }

        public File(string Name, string MD5)
        {
            this.Name = Name;
            this.MD5 = MD5;
        }
    }
}
