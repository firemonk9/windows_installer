using Ionic.Zip;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dataq.Installer
{
    public static class InstallManager
    {
        private const string SQL_QUERY_URL = "https://dataqio.s3.amazonaws.com/mysql_setup.sql";
        private const string NGINX_CONF_FILE__URL = "https://dataqio.s3.amazonaws.com/nginx.conf";

        private static List<Package> packages;
        private static List<Package> failedPackages;
        public static string ErrorMessage { get; private set; }
        public static int ProgressPercentage { get; private set; }
        public static int DownloadPercentage { get; private set; }
        public static bool IsRunning { get; set; }
        public static string CurrentTitle { get; private set; }


        static InstallManager()
        {

            packages = new List<Package>()
            {
                 new Package(){ Name="DataqServiceManager.zip",Description="Dataq Service Manager",
                    Url="https://dataqio.s3.amazonaws.com/windows/DataqServiceManager.zip"},
                new Package(){ Name="nginx-1.20.2.zip",Description="Nginx Web Server",
                    Url="https://nginx.org/download/nginx-1.20.2.zip"},
                new Package(){ Name="hadoop-2.6.0.zip",Description="Hadoop",
                    Url="https://dataqio.s3.amazonaws.com/hadoop-2.6.0.zip"},
                 new Package(){ Name="mariadb-10.6.5-winx64.msi",Description="MariaDB",
                    Url="https://dataqio.s3.amazonaws.com/windows/mariadb-10.6.5-winx64.msi",Args="/i {0} SERVICENAME=MySQL ADDLOCAL=ALL PASSWORD=dataq /qn",},
                new Package(){ Name="spark-2.4.7-bin-hadoop2.6.zip",Description="Spark",
                    Url="https://dataqio.s3.amazonaws.com/windows/spark-2.4.7-bin-hadoop2.6.zip"},
                new Package(){ Name="dataq.zip",Description="Dataq App",
                    Url="https://dataqio.s3.amazonaws.com/dataq.zip"},
                new Package(){ Name="amazon-corretto-8.332.08.1-windows-x64-jdk.msi",Description="Java 8 Development Kit",
                    Url="https://corretto.aws/downloads/resources/8.332.08.1/amazon-corretto-8.332.08.1-windows-x64-jdk.msi",Args="/i {0} /qn /norestart ALLUSERS=2"},
                new Package(){ Name="vcredist_x64.exe",Description="VC++ Redistributable"
                ,Url="https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-B6A430A6BFFC/vcredist_x64.exe",Args="/install /quiet /norestart "}
            };
        }

        public static async void StartInstalling()
        {
            IsRunning = true;

            Logger.LogEvent("Install process begins..");
            foreach (var pack in packages)
            {

                CurrentTitle = "Downloading " + pack.Description;
                var downloader = new WebClient();

                downloader.DownloadProgressChanged += DownloadProgressChanged;
                ProgressPercentage = 0;

                var tempFile = Path.Combine(Path.GetTempPath(), pack.Name);
                if (File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }

                try
                {
                    Logger.LogEvent("Downloading package "+ pack.Name);
                    await downloader.DownloadFileTaskAsync(pack.Url, tempFile);
                }
                catch (Exception ex)
                {
                    Logger.LogEvent("ERROR: Downloading package " + pack.Name);
                    Logger.LogEvent("Details:");
                    Logger.LogEvent(ex.Message);
                    Logger.LogEvent("__________________________________");

                    ErrorMessage = ex.Message;
                    continue;
                }
                downloader.DownloadProgressChanged -= DownloadProgressChanged;
                downloader.Dispose();


                CurrentTitle = "Installing " + pack.Description;
                if (pack.Name.EndsWith(".zip"))
                {
                    ProgressPercentage = 0;
                    Logger.LogEvent("Extracting archive '" + pack.Name + "' to 'C:\\Dataq\\'");

                    ZipFile zip = null;

                    try
                    {
                        zip = new ZipFile(tempFile);
                        zip.ExtractProgress += Zip_ExtractProgress;
                        zip.ExtractAll("C:\\Dataq", ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch (BadReadException badReadExc)
                    {
                        Logger.LogEvent("ERROR: File is corrupted: " + tempFile);
                        Logger.LogEvent("Details:");
                        Logger.LogEvent(badReadExc.Message);
                        Logger.LogEvent("__________________________________");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEvent("ERROR: Extracting package " + pack.Name);
                        Logger.LogEvent("Details:");
                        Logger.LogEvent(ex.Message);
                        Logger.LogEvent("__________________________________");
                    }

                    if (zip != null)
                    {
                        zip.ExtractProgress -= Zip_ExtractProgress;
                        zip.Dispose();
                    }
                }
                else
                {
                    Process p = new Process();
                    if (pack.Name.EndsWith(".exe"))
                    {
                        p.StartInfo.FileName = tempFile;
                        p.StartInfo.Arguments = pack.Args;
                    }
                    else if (pack.Name.EndsWith(".msi"))
                    {
                        p.StartInfo.FileName = "msiexec";
                        p.StartInfo.Arguments = string.Format(pack.Args, "\"" + tempFile + "\"");
                    }
                    else
                    {
                        break;
                    }

                    Logger.LogEvent("Running command '" + p.StartInfo.FileName + " with arguments '" + p.StartInfo.Arguments + "'");
                    p.Start();
                    p.WaitForExit();
                }

                try
                {
                    File.Delete(tempFile);
                }
                catch { }
            }

           
            CurrentTitle = "Configuring programs.. ";
            Logger.LogEvent("Configuring programs..");
            ConfigurePrograms();
            ProgressPercentage = 100;
            IsRunning = false;

            Logger.LogEvent("Installation completed!");
        }

        public static void ConfigurePrograms()
        {
            var lines = "|127.0.0.1   dq-frontend|127.0.0.1   db|127.0.0.1   myapp|127.0.0.1   dms|127.0.0.1   dms-em|127.0.0.1   dmsspark|127.0.0.1   dq-node|127.0.0.1   sql-parse".Split('|');
            List<string> insertLines = new List<string>();
            var existingLines = File.ReadAllLines(@"C:\Windows\System32\drivers\etc\hosts").ToList();

            Logger.LogEvent("Appending values to 'hosts' file");
            foreach (var line in lines)
            {
                if (existingLines.Exists(l => l.Contains(line)))
                    continue;

                insertLines.Add(line);
            }

            try
            {
                File.AppendAllLines(@"C:\Windows\System32\drivers\etc\hosts", insertLines);
            }catch(Exception ex)
            {
                Logger.LogEvent("ERROR: Appending values to 'hosts' file");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");
            }


            var envs = new Dictionary<string, string>()
        {
            { "JAVA_HOME","C:/Program Files/Amazon Corretto/jdk1.8.0_332"},//Backslash /
            { "SPARK_HOME",@"C:/Dataq/spark-2.4.7-bin-hadoop2.6"},//Backslash /
            { "HADOOP_HOME",@"C:\Dataq\hadoop-2.6.0"},
            { "NGINX_SERVER",@"C:\Dataq\nginx-1.20.2\nginx.exe"},
            { "DATAQ_HOME","C:/Dataq/dataq"},//Backslash /
        };


            Logger.LogEvent("Adding env variables..");

            foreach (var env in envs)
            {
                try
                {
                    Environment.SetEnvironmentVariable(env.Key, env.Value, EnvironmentVariableTarget.Machine);
                }
                catch (Exception ex)
                {
                    Logger.LogEvent("ERROR: Adding env variable '" + env.Key + " -> " + env.Value + "'");
                    Logger.LogEvent("Details:");
                    Logger.LogEvent(ex.Message);
                    Logger.LogEvent("__________________________________");
                }
            }


            Process p = new Process();
            p.StartInfo.FileName = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe";
            p.StartInfo.Arguments = @"C:\Dataq\DataqServiceManager\dsm.exe";

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;


            Logger.LogEvent("Registering 'DataqServiceManager' as a windows service..");
            try
            {
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {

                Logger.LogEvent("ERROR: Registering 'DataqServiceManager' as a windows service..");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");
            }


            Logger.LogEvent("Updating nginx server conf file");
            try
            {
                var text = new WebClient().DownloadString(NGINX_CONF_FILE__URL);

                File.WriteAllText(@"C:\Dataq\nginx-1.20.2\conf\nginx.conf",
                    text.Replace("%DATAQ_UI_PATH%", "C:/Dataq/dataq/ui/"));
            }
            catch (Exception ex)
            {
                Logger.LogEvent("ERROR: Updating nginx server conf file");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");
            }

            Logger.LogEvent("Connecting to MariaDB local server..");
            MySqlConnection conn;
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = "server=127.0.0.1;uid=root;pwd=dataq;";
                conn.Open();
            }
            catch (Exception ex)
            {
                Logger.LogEvent("ERROR: Connecting to MariaDB local server");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");

                return;
            }


            string sql = null;
            Logger.LogEvent("Fetching database schema from the internet");
            try
            {
                sql = new WebClient().DownloadString(SQL_QUERY_URL);
            }
            catch (Exception ex)
            {
                Logger.LogEvent("ERROR: Fetching database schema from the internet");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");

                return;
            }

            //Logger.LogEvent("Downloading database schema");
            Logger.LogEvent("Creating database schema..");
            try
            {
                var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogEvent("ERROR: Creating database schema");
                Logger.LogEvent("Details:");
                Logger.LogEvent(ex.Message);
                Logger.LogEvent("__________________________________");
                return;
            }

            Logger.LogEvent("Database schema created!");
        }
        private static void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EntriesTotal > 0)
            {
                ProgressPercentage = (e.EntriesExtracted * 100) / e.EntriesTotal;
            }
        }
        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
        }
    }
}
