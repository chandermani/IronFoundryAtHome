using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.WindowsAzure;
using System.IO;
using Entities;
using System.Threading.Tasks;
using NLog;

namespace AtHomeWebRole
{
    public class Global : System.Web.HttpApplication
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        void Application_Start(object sender, EventArgs e)
        {
            //Do these startup Tasks if not running on azure. For Azure use the RoleEntryPoint class. 
            if (!ApplicationSettings.RunningOnAzure)
            {
                CopyAtHomeBinaries(GetDriveStorage());
                StartClient();
            }
        }

        private void StartClient()
        {
            FoldingClient client = FoldingClientFactory.CreateFoldingClient();
            Task task = Task.Factory.StartNew(() =>
            {
                if (client.Identity == null)
                {
                    logger.Info("Since identity is null this is the first run. Client would started later");
                    return;
                }
                logger.Info("Starting AtHome Client.");
                client.Launch();

            });
        }

        private void CopyAtHomeBinaries(DriveStorageDetails storageDetails)
        {

            string copyAtHomeBinariesFrom = Server.MapPath("/binaries");
            string copyAtHomeBinariesTo = string.Format("{0}{1}", storageDetails.RootPath, "client");
            if (!Directory.Exists(copyAtHomeBinariesTo))
                Directory.CreateDirectory(copyAtHomeBinariesTo);
            logger.Info("Starting to copy AtHome binaries");
            foreach (var file in Directory.EnumerateFiles(copyAtHomeBinariesFrom))
            {
                String fileName = System.IO.Path.GetFileName(file);
                logger.Info("Copying file {0}", fileName);
                String destFile = System.IO.Path.Combine(copyAtHomeBinariesTo, fileName);
                if (File.Exists(destFile)) continue;
                System.IO.File.Copy(file, destFile, true);
            }
            logger.Info("Done copying AtHome Binaries");
        }

        private DriveStorageDetails GetDriveStorage()
        {
            return new DriveStorageDetails()
            {
                Name = "Temp",
                RootPath = Server.MapPath("/"),
            };
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            var exception = Server.GetLastError();
            System.Diagnostics.Trace.TraceError(exception.Message);

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
