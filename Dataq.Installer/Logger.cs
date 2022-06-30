using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataq.Installer
{
    public static class Logger
    {
        public static string LogFile { get; set; }

        public static void LogEvent(string message)
        {
           File.AppendAllLines(LogFile, new string[] { message });
        }
    }
}
