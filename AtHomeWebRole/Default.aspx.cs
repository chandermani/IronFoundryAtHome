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
using System.Threading.Tasks;

namespace AtHomeWebRole
{
    public partial class _Default : System.Web.UI.Page
    {
        // Other folding teams can modify team number and passkey below
        private const String TEAM_NUMBER = "184157";
        private const String PASSKEY = "";
        private IAtHomeClientDataRepository _clientDataRepo;
        public _Default()
        {
            if (ApplicationSettings.RunningOnAzure)
                _clientDataRepo = new AzureAtHomeClientDataRepository(ApplicationSettings.DataConnectionString);
            else
                _clientDataRepo = new MongoAtHomeDataRepository();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // hide all the panels
            var panels = from System.Web.UI.Control c in this.Controls
                         where c.GetType() == pnlInput.GetType()
                         select c;
            panels.ToList().ForEach(c => c.Visible = false);

            // set some literal values that may appear on the pageL 
            litConnString.Text = ApplicationSettings.DataConnectionString;

            try
            {
                // backdoor to reset the client table
                if (Request.QueryString["reset"] != null)
                {
                    _clientDataRepo.Clear();
                } 

                // if there's a record in client table, redirect to status page
                if (FoldingClientFactory.CreateFoldingClient().Identity!= null)
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
            catch (Exception ex)
            {
                pnlAccountError.Visible = true;
            }
        }

        protected void cbStart_Click(object sender, EventArgs e)
        {
            // create/confirm client table exists
            ClientInformation clientInfo = new ClientInformation(
                            txtName.Text,
                            PASSKEY,
                            TEAM_NUMBER,
                            Double.Parse(txtLatitudeValue.Value),
                            Double.Parse(txtLongitudeValue.Value),
                            Request.ServerVariables["SERVER_NAME"]);
            _clientDataRepo.Save(clientInfo);
            // redirect to the status page
            if (!ApplicationSettings.RunningOnAzure)
            {
                Task task = Task.Factory.StartNew(() =>
                     {
                         try
                         {
                             FoldingClient client = FoldingClientFactory.CreateFoldingClient();
                             client.Launch();
                         }
                         catch (Exception ex)
                         {
                             System.Diagnostics.Trace.TraceError(ex.Message);
                         }
                     });
            }
            Response.Redirect("/Status.aspx");

        }
    }
}