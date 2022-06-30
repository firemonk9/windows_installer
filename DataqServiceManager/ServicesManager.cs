using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DataqServiceManager
{
    public partial class ServicesManager : ServiceBase
    {
        public ServicesManager()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(() => StartServices());
        }
        protected override void OnStop()
        {
           StopServices();
        }

        private void StartServices()
        {
            ServiceManager.LogEvent("Service is started at " + DateTime.Now);
            //ServiceManager.LogEvent("Starting services..");

            try
            {
                ServiceManager.StartNginx(1);

                StartAzkaban();
                StartOtherServices();
                

                //Task.Factory.StartNew(() =>
                //{
                //    ServiceManager.LogEvent("Running Task ==> Starting services");
                //    StartOtherServices();
                //});

                //Task.Factory.StartNew(() =>
                //{
                //    ServiceManager.LogEvent("Running Task ==> Starting Azkaban App");
                //    StartAzkaban();
                //});

            }
            catch (Exception ex)
            {
                ServiceManager.LogEvent("ERR: Failed to start one of the services..");
                ServiceManager.LogEvent("Ex ==> " + ex.Message);
            }
        }

        private void StartAzkaban()
        {
            ServiceManager.LogEvent("Starting Azkaban App at " + DateTime.Now);
            ServiceManager.StartAzkaban();
            Thread.Sleep(5 * 1000);
            ServiceManager.GeServicesState();

            if (!ServiceManager.IsAzkabanRunning)
            {
                ServiceManager.LogEvent("Azkaban App failed to start, retrying again.." + DateTime.Now);
                ServiceManager.StartAzkaban();
                ServiceManager.LogEvent("Checking Azkaban App status.. " + DateTime.Now);
                Thread.Sleep(5 * 1000);
                ServiceManager.GeServicesState();

                if (!ServiceManager.IsAzkabanRunning)
                {
                    ServiceManager.LogEvent("Azkaban App failed to start again!" + DateTime.Now);
                }
                else
                {
                    ServiceManager.LogEvent("Azkaban App started with PID:" + ServiceManager.AZKABAN_PID + " at " + DateTime.Now);
                }
            }
            else
            {
                ServiceManager.LogEvent("Azkaban App started with PID:" + ServiceManager.AZKABAN_PID + " at " + DateTime.Now);
            }


        }

        private void StartOtherServices()
        {
            ServiceManager.LogEvent("Starting additional services at " + DateTime.Now);
            ServiceManager.StartServices(true);
            //ServiceManager.LogEvent("Services started");

            ServiceManager.LogEvent("Waiting for services to start " + DateTime.Now);
            Thread.Sleep(10 * 1000);
            ServiceManager.LogEvent("Checking services status.. " + DateTime.Now);
            ServiceManager.GeServicesState();

            if (!ServiceManager.AreServicesRunning)
            {
                ServiceManager.LogEvent("Additional services failed to start, retrying again..");
                ServiceManager.StartServices(true);
                ServiceManager.LogEvent("Waiting for services to start " + DateTime.Now);
                Thread.Sleep(10 * 1000);
                ServiceManager.GeServicesState();

                if (!ServiceManager.AreServicesRunning)
                {
                    ServiceManager.LogEvent("Additional services failed to start again!");
                }
                else
                {
                    ServiceManager.LogEvent(string.Format(
                    "Additional services started with the following IDs:  dms:{0}, mds-em:{1}, dms_sample:{2}",
                    ServiceManager.DMS_PID, ServiceManager.DMS_EM_PID, ServiceManager.DMS_SAMPLE_PID));
                }
            }
            else
            {

                ServiceManager.LogEvent(string.Format(
                    "Additional services started with the following IDs:  dms:{0}, mds-em:{1}, dms_sample:{2}",
                    ServiceManager.DMS_PID, ServiceManager.DMS_EM_PID, ServiceManager.DMS_SAMPLE_PID));
            }
        }
        private void StopServices()
        {
            ServiceManager.LogEvent("Stoping services..");
            try
            {
                ServiceManager.StartNginx(0);
                ServiceManager.StopAzkaban();
                ServiceManager.StartServices(false);
            }
            catch (Exception ex)
            {
                ServiceManager.LogEvent("Failed to stop one of the services..");
                ServiceManager.LogEvent("Ex ==> " + ex.Message);
            }

            ServiceManager.LogEvent("Service is stopped at " + DateTime.Now);
        }
    }
}
