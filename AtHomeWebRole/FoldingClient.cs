using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace AtHomeWebRole
{
    public class FoldingClientStatus
    {
        public String Name { get; set; }
        public String Tag { get; set; }
        public String DownloadTime { get; set; }
        public DateTime DueTime { get; set; }
        public Int32 Progress { get; set; }
        public Boolean HasParseError { get; set; }

        public FoldingClientStatus()
        {
            this.HasParseError = true;
        }

        public override string ToString()
        {
            return String.Format("Status: {0}-{1} {2}% complete",
                this.Name, this.Tag, this.Progress);
        }
    }

    public class FoldingClient
    {
        private ClientInformation Identity { get; set; }
        public static ClientInformation GetClientInformation()
        {
            ClientInformation clientInfo = null;
            try
            {
                // access client table in Azure storage
                CloudStorageAccount cloudStorageAccount =
                    CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
                var ctx = new ClientDataContext(
                    cloudStorageAccount.TableEndpoint.ToString(),
                    cloudStorageAccount.Credentials);

                // return the first (and only) record or nothing
                if (cloudStorageAccount.CreateCloudTableClient().DoesTableExist("client"))
                    clientInfo = ctx.Clients.FirstOrDefault<ClientInformation>();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(String.Format("Exception when accessing client data: {0} | {1}", ex.Message, ex.StackTrace));
                clientInfo = null;

                throw;
            }

            return clientInfo;
        }

        public FoldingClient(ClientInformation clientInfo)
        {
            this.Identity = clientInfo;
        }

        public void WriteConfigFile()
        {
            String baseSettings =
"[settings]{2}username={0}{2}team={1}{2}asknet=no{2}machineid=1{2}bigpackets=big{2}local=1{2}{3}[http]{2}active=no{2}host=localhost{2}port=8080{2}usereg=no{2}[clienttype]{2}memory=1024{2}type=0{2}";

            // get local file storage for configuration file
            LocalResource foldingIo = RoleEnvironment.GetLocalResource("ClientStorage");
            String targetPath = String.Format(@"{0}client\client.cfg", foldingIo.RootPath);

            // write the config file
            using (StreamWriter sw = System.IO.File.CreateText(targetPath))
            {
                sw.Write(String.Format(baseSettings, Identity.UserName, Identity.Team, "\u000A",
                    (Identity.PassKey.Length == 0) ? "" : String.Format("passkey={0}{1}", Identity.PassKey, "\u000A")));
                sw.Close();
            }
        }

        public FoldingClientStatus ReadStatusFile()
        {
            // create new status record
            FoldingClientStatus status = new FoldingClientStatus();

            // get local file storage for configuration file
            LocalResource foldingIo = RoleEnvironment.GetLocalResource("ClientStorage");
            String targetPath = String.Format(@"{0}client\unitinfo.txt", foldingIo.RootPath);

            // check for the status file
            if (System.IO.File.Exists(targetPath))
            {
                String[] contents = null;

                // read the workunit progress file
                using (StreamReader sr = new StreamReader(targetPath))
                {
                    contents = sr.ReadToEnd().
                                 Split(new String[] { Environment.NewLine }, StringSplitOptions.None);
                    sr.Close();
                }

                if (contents != null)
                {
                    String contentLine;

                    // presume all info for status record is parsed correctly
                    status.HasParseError = false;

                    // match name
                    contentLine = contents.Where((c) => c.StartsWith("Name:")).FirstOrDefault();
                    Regex r = new Regex(@"^Name: (.*)");
                    Match m = r.Match(contentLine);
                    if (m.Success)
                    {
                        status.Name = m.Groups[1].Value;
                    }
                    else
                    {
                        status.HasParseError = true;
                    }

                    // match tag
                    contentLine = contents.Where((c) => c.StartsWith("Tag:")).FirstOrDefault();
                    r = new Regex(@"^Tag: (.*)");
                    m = r.Match(contentLine);
                    if (m.Success)
                    {
                        status.Tag = m.Groups[1].Value;
                    }
                    else
                    {
                        status.HasParseError = true;
                    }

                    // match Progress
                    contentLine = contents.Where((c) => c.StartsWith("Progress:")).FirstOrDefault();
                    r = new Regex(@"^Progress: ([0-9]*)%");
                    m = r.Match(contentLine);
                    if (m.Success)
                    {
                        status.Progress = Int32.Parse(m.Groups[1].Value);
                    }
                    else
                    {
                        status.HasParseError = true;
                    }

                    // match Download time
                    contentLine = contents.Where((c) => c.StartsWith("Download time:")).FirstOrDefault();
                    r = new Regex(@"^Download time: (.*)");
                    m = r.Match(contentLine);
                    if (m.Success)
                    {
                        status.DownloadTime = m.Groups[1].Value;
                    }
                    else
                    {
                        status.HasParseError = true;
                    }

                    if (status.HasParseError)
                        Trace.TraceError("Error parsing status file: {0}{1}",
                            Environment.NewLine,
                            String.Join(Environment.NewLine, contents));
                }
            }
            else
            {
                Trace.TraceError(String.Format("Status file not found: {0}", targetPath));
            }

            return status;
        }

        public void UpdateServerStatus(FoldingClientStatus status)
        {

            string url = string.Format("{0}/ping",
                RoleEnvironment.GetConfigurationSettingValue("PingServer"));

            string data = string.Format("username={0}&passkey={1}&instanceid={2}&lat={3}&long={4}&foldingname={5}&foldingtag={6}&foldingprogress={7}&deploymentid={8}&servername={9}&downloadtime={10}",
                HttpUtility.UrlEncode(Identity.UserName),
                Identity.PassKey,
                RoleEnvironment.CurrentRoleInstance.Id,
                Identity.Latitude,
                Identity.Longitude,
                HttpUtility.UrlEncode(status.Name),
                HttpUtility.UrlEncode(status.Tag),
                status.Progress,
                RoleEnvironment.DeploymentId,
                Identity.ServerName,
                HttpUtility.UrlEncode(status.DownloadTime)
                );

            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";

                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
                req.ContentLength = bytes.Length;

                using (System.IO.Stream stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(String.Format("Error sending ping: {0} | {1} | {2}", ex.Message, url, ex.StackTrace));
            }
        }

        public void UpdateLocalStatus(FoldingClientStatus status)
        {
            var cloudStorageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // ensure workunit table exists
            var cloudClient = new CloudTableClient(
                cloudStorageAccount.TableEndpoint.ToString(),
                cloudStorageAccount.Credentials);
            cloudClient.CreateTableIfNotExist("workunit");

            // select info for given workunit
            var ctx = new ClientDataContext(
                    cloudClient.BaseUri.ToString(),
                    cloudClient.Credentials);
            var workUnit = (from w in ctx.WorkUnits.ToList<WorkUnit>()
                            where w.PartitionKey == RoleEnvironment.CurrentRoleInstance.Id &&
                              w.RowKey == w.MakeKey(status.Name, status.Tag, status.DownloadTime)
                            select w).FirstOrDefault<WorkUnit>();

            // if it's a new one, add it
            if (workUnit == null)
            {
                workUnit = new WorkUnit(status.Name, status.Tag, status.DownloadTime, RoleEnvironment.CurrentRoleInstance.Id) { Progress = status.Progress, StartTime = DateTime.UtcNow };
                ctx.AddObject("workunit", workUnit);
            }

            // otherwise, update it
            else
            {
                workUnit.Progress = status.Progress;
                if (workUnit.Progress == 100)
                    workUnit.CompleteTime = DateTime.UtcNow;
                ctx.UpdateObject(workUnit);
            }
            ctx.SaveChanges();
        }

        public void Launch()
        {

            // write the configuration file with user information
            WriteConfigFile();

            // get path to the Folding@home client application
            LocalResource foldingIo = RoleEnvironment.GetLocalResource("ClientStorage");
            String targetPath = string.Format(@"{0}client", foldingIo.RootPath);
            String targetExecutable = string.Format(@"{0}client\{1}", foldingIo.RootPath,
                RoleEnvironment.GetConfigurationSettingValue("ClientEXE"));

            // get progress polling interval (default to 15 minutes)
            Int32 pollingInterval;
            if (!Int32.TryParse(
                RoleEnvironment.GetConfigurationSettingValue("PollingInterval"),
                out pollingInterval))
                pollingInterval = 15;

            // 
            // setup process
            ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = targetExecutable,
                    WorkingDirectory = targetPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = "-oneunit"
                };

            // loop while there's a client info record in Azure table storage
            while (this.Identity != null)
            {

                // start a work unit
                using (Process exeProcess = Process.Start(startInfo))
                {

                    while (!exeProcess.HasExited)
                    {
                        // get current status
                        FoldingClientStatus status = ReadStatusFile();

                        // update local status table (workunit table in Azure storage)
                        if (!status.HasParseError)
                        {
                            UpdateLocalStatus(status);
                            UpdateServerStatus(status);
                        }

                        Thread.Sleep(TimeSpan.FromMinutes(pollingInterval));
                    }

                    // when work unit completes successfully
                    if (exeProcess.ExitCode == 0)
                    {
                        // make last update for completed role
                        FoldingClientStatus status = ReadStatusFile();

                        if (!status.HasParseError)
                        {
                            UpdateLocalStatus(status);
                            UpdateServerStatus(status);
                        }
                    }
                    else
                    {
                        Trace.TraceError(String.Format("Folding@home process has exited with code {0}",
                            exeProcess.ExitCode));

                        // this will leave orphan progress record in the Azure table
                    }

                    // re-check client table to make sure there's still a record there, if not, that's
                    // the cue to stop the folding process
                    this.Identity = GetClientInformation();
                }
            }
        }
    }
}
