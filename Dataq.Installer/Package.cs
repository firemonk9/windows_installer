using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataq.Installer
{
    class Package
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public string Args { get; set; }//Zip
    }
}
