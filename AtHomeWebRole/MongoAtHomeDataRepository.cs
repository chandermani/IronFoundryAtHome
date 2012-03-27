using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Entities;
using MongoDB.Driver.Builders;

namespace AtHomeWebRole
{
    public class MongoAtHomeDataRepository : IAtHomeClientDataRepository
    {
        private MongoServer _mongoServer;
        private MongoDatabase _mongoDatabase;
        private string _dbName = "athomedb";
        public MongoAtHomeDataRepository()
        {
            _mongoServer = MongoServer.Create(ApplicationSettings.MongoDBConnectionString);
            _mongoDatabase = _mongoServer.GetDatabase(_dbName);

            if (!_mongoDatabase.CollectionExists("clientinformation"))
                _mongoDatabase.CreateCollection("clientinformation");

            if (!_mongoDatabase.CollectionExists("workunit"))
                _mongoDatabase.CreateCollection("workunit");
        }

        public void Save(ClientInformation clientInfo)
        {
            MongoCollection<ClientInformation> coll = _mongoDatabase.GetCollection<ClientInformation>("clientinformation");
            coll.Insert(clientInfo);
        }

        public void Save(WorkUnit workUnit)
        {
            MongoCollection<WorkUnit> coll = _mongoDatabase.GetCollection<WorkUnit>("workunit");
            coll.Insert(workUnit);
        }

        public void Update(WorkUnit workUnit)
        {
            MongoCollection<WorkUnit> coll = _mongoDatabase.GetCollection<WorkUnit>("workunit");
            var query = Query.And(Query.EQ("PartitionKey", workUnit.PartitionKey), Query.EQ("RowKey", workUnit.RowKey));
            var docUpdate = MongoDB.Driver.Builders.Update.Replace<WorkUnit>(workUnit);
            coll.Update(query, docUpdate);
        }

        public ClientInformation LoadClientInformation()
        {
            MongoCollection<ClientInformation> coll = _mongoDatabase.GetCollection<ClientInformation>("clientinformation");
            return coll.FindOne();
        }

        public WorkUnit GetWorkUnit(string key, string subKey)
        {
            MongoCollection<WorkUnit> coll = _mongoDatabase.GetCollection<WorkUnit>("workunit");
            var query = Query.And(Query.EQ("PartitionKey", key), Query.EQ("RowKey", subKey));
            return coll.FindOne(query);
        }

        public void Clear()
        {
            _mongoDatabase.GetCollection<WorkUnit>("workunit").Drop();            
            _mongoDatabase.GetCollection<ClientInformation>("clientinformation").Drop();

        }

        public List<WorkUnit> AllWorkUnits()
        {
            MongoCollection<WorkUnit> coll = _mongoDatabase.GetCollection<WorkUnit>("workunit");
            return coll.FindAll().ToList();
        }
    }
    
}