using System;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Entities
{
    public class ClientInformation : TableServiceEntity
    {
        public String UserName { get; set; }
        public String Team { get; set; }
        public String ServerName { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public String PassKey { get; set; }

        public ClientInformation() { }
        
        public ClientInformation(String userName, String passKey, String teamName,
            Double latitude, Double longitude, String serverName)
        {
            this.UserName = userName;
            this.PassKey = passKey;
            this.Team = teamName;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.ServerName = serverName;

            this.PartitionKey = this.UserName;
            this.RowKey = this.PassKey;
        }
    }
    public class WorkUnit : TableServiceEntity
    {
        public String Name { get; set; }
        public String Tag { get; set; }
        public String InstanceId { get; set; }
        public Int32 Progress { get; set; }
        public String DownloadTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompleteTime { get; set; }

        public WorkUnit() { }
        public WorkUnit(String name, String tag, String downloadTime, String instanceId)
        {
            this.Name = name;
            this.Tag = tag;
            this.Progress = 0;
            this.InstanceId = instanceId;
            this.StartTime = DateTime.UtcNow;
            this.CompleteTime = null;
            this.DownloadTime = downloadTime;

            this.PartitionKey = this.InstanceId;
            this.RowKey = MakeKey(this.Name, this.Tag, this.DownloadTime);
        }

        public String MakeKey(String n, String t, String d)
        {
            return n + "|" + t + "|" + d;
        }
    }
    public class ClientDataContext : TableServiceContext
    {

        public ClientDataContext(String baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        public IQueryable<ClientInformation> Clients
        {
            get
            {
                return this.CreateQuery<ClientInformation>("client");
            }
        }

        public IQueryable<WorkUnit> WorkUnits
        {
            get
            {
                return this.CreateQuery<WorkUnit>("workunit");
            }
        }
    }
}