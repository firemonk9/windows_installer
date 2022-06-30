using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using MySqlConnector;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Dataq.Installer
{
    public partial class frmMain : Form
    {
        Timer timer;
        public frmMain()
        {
            InitializeComponent();
            timer = new Timer();

            timer.Tick += Timer_Tick;
            timer.Interval = 100;

            Logger.LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install.log");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (InstallManager.IsRunning)
            {
                lblPackageName.Text = InstallManager.CurrentTitle;
                lblPercent.Text = InstallManager.ProgressPercentage.ToString() + "%";
                progBar.Value = InstallManager.ProgressPercentage;
            }
            else
            {
                lblPercent.Text = "100%";
                progBar.Value = 100;
                timer.Stop();
                lblPackageName.Text = "Setup completed!";

                grpInst.Visible = true;
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (File.Exists(Logger.LogFile))
            {
                try
                {
                    File.Delete(Logger.LogFile);
                }
                catch { }
            }

            if (!Directory.Exists("C:\\Dataq"))
            {
                Logger.LogEvent("creating 'Dataq' folder in 'C:' drive.");
                Directory.CreateDirectory("C:\\Dataq");
            }

            InstallManager.IsRunning = true;
            Task.Factory.StartNew(() => InstallManager.StartInstalling());

            timer.Start();
            btnInstall.Visible = false;
            
        }

    }
}
