using System;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Entities;

namespace AtHomeWebRole
{
    public partial class Status : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var clientInfo = FoldingClient.GetClientInformation();
            litName.Text = clientInfo != null ? clientInfo.UserName : "Hello, unidentifiable user";

            // ensure workunit table exists
            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            CloudTableClient cloudClient = cloudStorageAccount.CreateCloudTableClient();

            if (cloudClient.DoesTableExist("workunit"))
            {
                ClientDataContext ctx = new ClientDataContext(
                    cloudClient.BaseUri.ToString(),
                    cloudClient.Credentials);
                var workUnitList = ctx.WorkUnits.ToList<WorkUnit>();

                InProgress.DataSource =
                    (from w in workUnitList
                     where w.CompleteTime == null
                     let duration = (DateTime.UtcNow - w.StartTime.ToUniversalTime())
                     orderby w.InstanceId ascending
                     select new
                     {
                         w.Name,
                         w.Tag,
                         w.InstanceId,
                         w.Progress,
                         Duration = String.Format("{0:#0} h {1:00} m",
                            duration.TotalHours, duration.Minutes)
                     });
                InProgress.DataBind();

                GridViewCompleted.DataSource =
                    (from w in workUnitList
                     where w.CompleteTime != null
                     let duration = (w.CompleteTime.Value.ToUniversalTime() - w.StartTime.ToUniversalTime())
                     orderby w.StartTime descending
                     select new
                     {
                         w.Name,
                         w.Tag,
                         w.StartTime,
                         Duration = String.Format("{0:#0} h {1:00} m",
                            duration.TotalHours, duration.Minutes)
                     });
                GridViewCompleted.DataBind();
            }

            litNoProgress.Visible = InProgress.Items.Count == 0;
            litCompletedTitle.Visible = GridViewCompleted.Rows.Count > 0;
        }
    }
}
