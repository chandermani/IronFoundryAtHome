using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics;

namespace AtHomeWebRole
{
    

    public class AzureAtHomeClientDataRepository : IAtHomeClientDataRepository
    {
        private const string CLIENT_INFO_TABLE_NAME="client";
        private const string WORK_UNIT_TABlE_NAME = "workunit";
        private CloudStorageAccount StorageAccount { get; set; }
        private CloudTableClient _tableClient;
        private ClientDataContext _dataContext;
        public AzureAtHomeClientDataRepository(string dataConnectionString)
        {
            StorageAccount =
                CloudStorageAccount.Parse(dataConnectionString);
            _tableClient = StorageAccount.CreateCloudTableClient();
            _dataContext = new ClientDataContext(
                   StorageAccount.TableEndpoint.ToString(),
                   StorageAccount.Credentials);
        }
        
        public void Save(ClientInformation clientInfo)
        {
            _tableClient.CreateTableIfNotExist(CLIENT_INFO_TABLE_NAME);
            // if table exists
            if (_tableClient.DoesTableExist(CLIENT_INFO_TABLE_NAME))
            {
                // add client info record
                _dataContext.AddObject(CLIENT_INFO_TABLE_NAME, clientInfo);
                _dataContext.SaveChanges();
            }
        }

        public ClientInformation LoadClientInformation()
        {
            try
            {

                // return the first (and only) record or nothing
                if (_tableClient.DoesTableExist(CLIENT_INFO_TABLE_NAME))
                    return _dataContext.Clients.FirstOrDefault<ClientInformation>();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(String.Format("Exception when accessing client data: {0} | {1}", ex.Message, ex.StackTrace));
                throw;
            }
            return null;
        }


        public void Clear()
        {
            _tableClient.DeleteTableIfExist("client");
            _tableClient.DeleteTableIfExist("workunit");
        }


        public void Save(WorkUnit workUnit)
        {
            
            _tableClient.CreateTableIfNotExist(WORK_UNIT_TABlE_NAME);
            // if table exists
            if (_tableClient.DoesTableExist(WORK_UNIT_TABlE_NAME))
            {
                // add client info record
                _dataContext.AddObject(WORK_UNIT_TABlE_NAME, workUnit);
                _dataContext.SaveChanges();
            }
        }


        public void Update(WorkUnit workUnit)
        {
            _tableClient.CreateTableIfNotExist(WORK_UNIT_TABlE_NAME);
            // select info for given workunit

            if (_tableClient.DoesTableExist(WORK_UNIT_TABlE_NAME))
            {
                _dataContext.UpdateObject(workUnit);
                _dataContext.SaveChanges();
            }
        }

        public WorkUnit GetWorkUnit(string key, string subKey)
        {
            
            _tableClient.CreateTableIfNotExist(WORK_UNIT_TABlE_NAME);
            // select info for given workunit
            var workUnit = (from w in _dataContext.WorkUnits.ToList<WorkUnit>()
                            where w.PartitionKey == key &&
                              w.RowKey == subKey
                            select w).FirstOrDefault<WorkUnit>();
            return workUnit;
        }


        public List<WorkUnit> AllWorkUnits()
        {
            return _dataContext.WorkUnits.ToList();
        }
    }
}
