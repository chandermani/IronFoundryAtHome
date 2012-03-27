using System;
using System.Linq;
using Entities;

namespace AtHomeWebRole
{
    public partial class Status : System.Web.UI.Page
    {
        private IAtHomeClientDataRepository _clientDataRepo;
        public Status()
        {
            if (ApplicationSettings.RunningOnAzure)
                _clientDataRepo = new AzureAtHomeClientDataRepository(ApplicationSettings.DataConnectionString);
            else
                _clientDataRepo = new MongoAtHomeDataRepository();
        }
        private FoldingClient _foldingClient;
        protected void Page_Load(object sender, EventArgs e)
        {
            _foldingClient = FoldingClientFactory.CreateFoldingClient();
            litName.Text = _foldingClient.Identity != null ? _foldingClient.Identity.UserName : "Hello, unidentifiable user";

            // ensure workunit table exists

            var workUnitList = _clientDataRepo.AllWorkUnits();

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

            litNoProgress.Visible = InProgress.Items.Count == 0;
            litCompletedTitle.Visible = GridViewCompleted.Rows.Count > 0;


        }
    }
}
