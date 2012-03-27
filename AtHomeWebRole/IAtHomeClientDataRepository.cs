using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entities;

namespace AtHomeWebRole
{
    public interface IAtHomeClientDataRepository
    {
        void Save(ClientInformation clientInfo);
        void Save(WorkUnit workUnit);
        void Update(WorkUnit workUnit);
        ClientInformation LoadClientInformation();
        WorkUnit GetWorkUnit(string key, string subKey);
        void Clear();
        List<WorkUnit> AllWorkUnits();
    }
}