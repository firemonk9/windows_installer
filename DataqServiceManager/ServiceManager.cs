using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace DataqServiceManager
{

    public static class ServiceManager
    {
        private static string NGINX_PATH = "%NGINX_SERVER%";//@"E:\DataQ\nginx-1.20.2\nginx.exe";
        private static string AZKABAN_CMD = "start_main_app.bat";//@"%DATAQ_HOME%\start_main_app.bat";
        private static string SHELL_CMD = "start_servicess.ps1";//@"%DATAQ_HOME%\start_servicess.ps1";
        private static string DATAQ_HOME = "DATAQ_HOME";

        public static int AZKABAN_PID = 0;
        public static int DMS_PID = 0;
        public static int DMS_EM_PID = 0;
        public static int DMS_SAMPLE_PID = 0;

        private static string LogFile;
        private static PowerShell ps;
        public static bool IsNginxRunning { get; private set; }
        public static bool IsAzkabanRunning 
        {
            get => AZKABAN_PID > 0;
        }
        public static bool AreServicesRunning
        {
            get => DMS_PID > 0 || DMS_EM_PID > 0 || DMS_SAMPLE_PID > 0;
        }

        static ServiceManager()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            LogFile = Path.Combine(path, "events.log");

            NGINX_PATH = Environment.GetEnvironmentVariable("NGINX_SERVER");

            if (string.IsNullOrWhiteSpace(NGINX_PATH) || !System.IO.File.Exists(NGINX_PATH))
            {
                LogEvent("ERR: 'NGINX_SERVER' env variable is not defined or file doesn't exist!");
                NGINX_PATH = null;
            }

            DATAQ_HOME = Environment.GetEnvironmentVariable("DATAQ_HOME");
            if (string.IsNullOrWhiteSpace(DATAQ_HOME))
               // !Directory.Exists(DATAQ_HOME.Replace('/', '\\')))
            {
                LogEvent("ERR: 'DATAQ_HOME' env variable is not defined or path doesn't exist!");
                AZKABAN_CMD = SHELL_CMD = null;
            }

            AZKABAN_CMD = Path.Combine(DATAQ_HOME,AZKABAN_CMD);
           if(!System.IO.File.Exists(AZKABAN_CMD))
            {
                LogEvent("command file \""+ AZKABAN_CMD + "\" doesn't exist in \""+ DATAQ_HOME+"\"");
                AZKABAN_CMD = null;
            }
            
            SHELL_CMD = Path.Combine(DATAQ_HOME, SHELL_CMD);
            if (!System.IO.File.Exists(SHELL_CMD))
            {
                LogEvent("command file \"" + SHELL_CMD + "\" doesn't exist in \"" + DATAQ_HOME + "\"");
                SHELL_CMD = null;
            }

        }
        public static void LogEvent(string message)
        {
            System.IO.File.AppendAllLines(LogFile, new string[] { message });
        }
        public static void RefreshServiceStatus()
        {
            GetNginxServiceState();
            GeServicesState();

        }
        public static int GetNginxServiceState()
        {
            IsNginxRunning = false;
            var nginx = Process.GetProcessesByName("nginx");
            if (nginx.Length == 0)
                return 0;
            else
            {
                var request = HttpWebRequest.Create("http://127.0.0.1");
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    IsNginxRunning = true;
                    NGINX_PATH = nginx[0].MainModule.FileName;

                    SaveSettings(NGINX_PATH);

                    return 1;
                }
                else
                    return -1;
            }
        }

        private static void SaveSettings(string data)
        {
            //var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataqServiceManager");
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataqServiceManager");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

           var settingsFile= Path.Combine(folder,"settings.conf");
           System.IO.File.WriteAllText(settingsFile, data); 
        }

        private static string LoadSettings()
        {
            //var settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataqServiceManager", "settings.conf");
            var settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataqServiceManager", "settings.conf");
            if (!System.IO.File.Exists(settingsFile))
                return null;

            return System.IO.File.ReadAllText(settingsFile);
        }
        public static void StartNginx(int state)//0:stop, 1:start 2:restart
        {
            if (NGINX_PATH == null)
                return;

            Process p = new Process();
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(NGINX_PATH);
            p.StartInfo.FileName = NGINX_PATH;
            if (state == 0)//stop
                p.StartInfo.Arguments = "-s quit";
            else if (state == 2)//restart
                p.StartInfo.Arguments = "-s reload";

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;


            LogEvent("querying nginx server with command "+ p.StartInfo.Arguments);
            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                LogEvent("Failed to execute nginx command");
                LogEvent(ex.Message);
                return;
            }

            LogEvent("nginx command run successfully.");

            if (state == 0)
            {
                ForceStopNginx();
            }

        }

        private static void ForceStopNginx()
        {
            var nginx = Process.GetProcessesByName("nginx");
            foreach (var n in nginx)
            {
                n.Kill();
            }
        }
        public static void StartAzkaban()
        {
            //var output = GetCMDOutput(AZKABAN_CMD.Replace("%DATAQ_HOME%", Environment.GetEnvironmentVariable("DATAQ_HOME")), false);
            if (AZKABAN_CMD == null)
            {
                LogEvent("Err: Azkaban Command not defined");
                return;
            }

            GetCMDOutput(AZKABAN_CMD,false);

            //LogEvent("Azkaban App started..");
        }

        public static void StopAzkaban()
        {
            StopProcess(AZKABAN_PID);

            var javaw = Process.GetProcessesByName("javaw.exe");
            foreach(var j in javaw)
            {
                try
                {
                    j.Kill();
                }
                catch (Exception ex)
                {

                }
               
            }
        }
        private static void StopProcess(int id)
        {
            try
            {
                Process.GetProcessById(id).Kill();
            }
            catch (Exception ex)
            {

            }
        }

        public static void StartServices(bool start)
        {
            if (start)
            {
                if (SHELL_CMD == null)
                {
                    LogEvent("Err: PowerSell Command not defined");
                    return;
                }

                try
                {
                    ps = PowerShell.Create();
                    ps.AddScript(System.IO.File.ReadAllText(SHELL_CMD));
                    ps.Invoke();
                    
                    //LogEvent("Additional services started..");
                }
                catch (Exception ex)
                {
                    LogEvent("ERR: Failed to execute PowerShell command!");
                    LogEvent("EXC: "+ex.Message);
                    LogEvent(ex.Message);
                }

            }
            else //stop services
            {
                try
                {
                    ps.Stop();
                }
                catch { }

                var pids = new int[] { DMS_PID, DMS_EM_PID, DMS_SAMPLE_PID };

                foreach (var p in pids)
                {
                    try
                    {
                        Process.GetProcessById(p).Kill();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                var java = Process.GetProcessesByName("java.exe");
                foreach (var j in java)
                {
                    try
                    {
                        j.Kill();
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }

     
        public static bool GeServicesState()
        {
            AZKABAN_PID = DMS_PID = DMS_EM_PID = DMS_SAMPLE_PID = 0;

            var output = GetCMDOutput("jps");
            if (string.IsNullOrWhiteSpace(output) || !output.Contains(Environment.NewLine))
                return false;

            var clean = output.Replace(Environment.NewLine, "*");
            var lines = clean.Split('*');

            try
            {
                AZKABAN_PID = int.Parse(lines.First(l => l.Contains("AzkabanSingleServer")).Split(' ')[0]);
            }
            catch { }

            try
            {
                DMS_PID = int.Parse(lines.First(l => l.Contains("dms-0")).Split(' ')[0]);
            }
            catch { }

            try
            {
                DMS_EM_PID = int.Parse(lines.First(l => l.Contains("dms-em")).Split(' ')[0]);
            }
            catch { }
            try
            {
                DMS_SAMPLE_PID = int.Parse(lines.First(l => l.Contains("dms_sample")).Split(' ')[0]);
            }
            catch { }

            return true;
        }

        private static string GetCMDOutput(string cmd, bool wait = true)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = cmd;
            p.StartInfo.CreateNoWindow = true;

            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                LogEvent("ERR: Failed to execute command => "+cmd);
                LogEvent("EXC: " + ex.Message);
                return null;
            }
            

            if (wait)
            {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                return output;
            }
            return string.Empty;
        }
        
    }
}
