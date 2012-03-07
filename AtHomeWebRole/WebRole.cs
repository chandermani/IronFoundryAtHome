using System.Threading;
using Entities;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;

namespace AtHomeWebRole
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            //setup the local file storage
            LocalResource foldingIo = RoleEnvironment.GetLocalResource("ClientStorage");
            String targetPath = String.Format(@"{0}client", foldingIo.RootPath);

            //copy files to temp storage
            String rootPath = Environment.GetEnvironmentVariable("RoleRoot");
            String sourcePath = String.Format(@"{0}\approot\bin\client", rootPath);

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            String[] files = System.IO.Directory.GetFiles(sourcePath);

            // Copy the files and overwrite destination files if they already exist.
            foreach (String s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                String fileName = System.IO.Path.GetFileName(s);
                String destFile = System.IO.Path.Combine(targetPath, fileName);
                System.IO.File.Copy(s, destFile, true);
            }

            return base.OnStart();
        }

        public override void Run()
        {
            // poll for the client information in Azure table every 30 seconds
            while (true)
            {
                try
                {
                    ClientInformation clientInfo = FoldingClient.GetClientInformation();
                    if (clientInfo != null)
                    {
                        FoldingClient client = new FoldingClient(clientInfo);
                        client.Launch();
                        break;
                    }
                }
                catch (Exception)
                {
                }
                Thread.Sleep(30000);
            }
        }
    }
}
