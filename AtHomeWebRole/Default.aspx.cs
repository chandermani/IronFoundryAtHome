using System;
using System.Linq;
using Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Net;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.IO;

namespace AtHomeWebRole
{
    public partial class _Default : System.Web.UI.Page
    {
        // Other folding teams can modify team number and passkey below
        private const String TEAM_NUMBER = "184157";
        private const String PASSKEY = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // hide all the panels
            var panels = from System.Web.UI.Control c in this.Controls
                         where c.GetType() == pnlInput.GetType()
                         select c;
            panels.ToList().ForEach(c => c.Visible = false);

            // set some literal values that may appear on the pageL 
            litConnString.Text = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            try
            {
                // backdoor to reset the client table
                if (Request.QueryString["reset"] != null)
                {
                    CloudStorageAccount cloudStorageAccount =
                        CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
                    cloudStorageAccount.CreateCloudTableClient().DeleteTableIfExist("client");
                    cloudStorageAccount.CreateCloudTableClient().DeleteTableIfExist("workunit");
                } 

                // if there's a record in client table, redirect to status page
                if (FoldingClient.GetClientInformation() != null)
                    Response.Redirect("/Status.aspx", true);

                if (!IsPostBack)
                {
                    // pull out the TLD of the server as the default folding client name
                    String serverName = Request.ServerVariables["SERVER_NAME"];
                    Int32 tldPos = serverName.IndexOf(".cloudapp.net");
                    txtName.Text = (tldPos >= 0) ? serverName.Substring(0, tldPos) : "localhost";

                    // try to populate the latitude/longitude input fields from GeoIP lookup
                    try
                    {
                        var latLongRequest = (HttpWebRequest)WebRequest.Create(
                            String.Format("http://myworldmaps.net/resolve/{0}",
                            Request.ServerVariables["REMOTE_ADDR"] == "127.0.0.1" ? String.Empty : Request.ServerVariables["REMOTE_ADDR"]));
                        latLongRequest.Timeout = 5000;
                        var latLongResponse = (HttpWebResponse)latLongRequest.GetResponse();
                        if (latLongResponse.StatusCode == HttpStatusCode.OK)
                        {
                            String rawJSONString = new StreamReader(latLongResponse.GetResponseStream()).ReadToEnd();
                            Dictionary<string, string> JSONData =
                                new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(rawJSONString);
                            txtLatitude.Text = JSONData["latitude"];
                            txtLongitude.Text = JSONData["longitude"];

                            txtLatitudeValue.Value = txtLatitude.Text;
                            txtLongitudeValue.Value = txtLongitude.Text;
                        }
                    }
                    catch (Exception)
                    {
                        txtLatitude.Text = String.Empty;
                        txtLongitude.Text = String.Empty;
                    }
                }

                // otherwise, show the form to kick off the folding process
                pnlInput.Visible = true;
            }
            catch (StorageClientException sce)
            {
                if ((sce.ErrorCode == StorageErrorCode.AccessDenied) || (sce.ErrorCode == StorageErrorCode.AuthenticationFailure))
                    pnlAccountForbidden.Visible = true;
                else
                    pnlAccountError.Visible = true;
            }
            catch (Exception)
            {
                pnlAccountError.Visible = true;
            }
        }

        protected void cbStart_Click(object sender, EventArgs e)
        {
            // create/confirm client table exists
            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            CloudTableClient cloudClient = cloudStorageAccount.CreateCloudTableClient();
            cloudClient.CreateTableIfNotExist("client");

            // if table exists
            if (cloudClient.DoesTableExist("client"))
            {
                // create a new client info record to persist to table storage
                ClientInformation clientInfo = new ClientInformation(
                            txtName.Text,
                            PASSKEY,
                            TEAM_NUMBER,
                            Double.Parse(txtLatitudeValue.Value),
                            Double.Parse(txtLongitudeValue.Value),
                            Request.ServerVariables["SERVER_NAME"]);

                // add client info record
                var ctx = new ClientDataContext(
                   cloudStorageAccount.TableEndpoint.ToString(),
                   cloudStorageAccount.Credentials);
                ctx.AddObject("client", clientInfo);
                ctx.SaveChanges();

                // redirect to the status page
                Response.Redirect("/Status.aspx");
            }
        }
    }
}