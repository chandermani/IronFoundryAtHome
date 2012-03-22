using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.WindowsAzure;
using System.IO;
using Entities;

namespace AtHomeWebRole
{
    public class Global : System.Web.HttpApplication
    {

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
            FoldingClient client = FoldingClientFactory.GetFoldingClient();
            client.Launch();
        }

        private void CopyAtHomeBinaries(DriveStorageDetails storageDetails)
        {
            string copyAtHomeBinariesFrom = Server.MapPath("/client");
            string copyAtHomeBinariesTo = string.Format("{0}{1}", storageDetails.RootPath, "client");
            if (!Directory.Exists(copyAtHomeBinariesTo))
                Directory.CreateDirectory(copyAtHomeBinariesTo);
            foreach (var file in Directory.EnumerateFiles(copyAtHomeBinariesFrom))
            {
                String fileName = System.IO.Path.GetFileName(file);
                String destFile = System.IO.Path.Combine(copyAtHomeBinariesTo, fileName);
                System.IO.File.Copy(file, destFile, true);
            }
        }

        private DriveStorageDetails GetDriveStorage()
        {
            return new DriveStorageDetails()
            {
                Name = "Temp",
                RootPath = Path.GetTempPath(),
            };
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

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
