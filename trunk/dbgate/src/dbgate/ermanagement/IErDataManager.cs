using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IErDataManager
    {
        void Load(IServerRoDbClass serverRoDbClass, IDataReader reader, IDbConnection con);

        void Save(IServerDbClass serverDbClass, IDbConnection con);

        ICollection<Object> Select(ISelectionQuery query,IDbConnection con );

        void ClearCache();

        void RegisterEntity(Type entityType, String tableName, ICollection<IField> fields);
    }
}