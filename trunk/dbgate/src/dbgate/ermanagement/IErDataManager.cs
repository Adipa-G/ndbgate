using System;
using System.Collections.Generic;
using System.Data;

namespace dbgate.ermanagement
{
    public interface IErDataManager
    {
        void Load(IServerRoDbClass serverRoDbClass, IDataReader reader, IDbConnection con);

        void Save(IServerDbClass serverDbClass, IDbConnection con);

        void ClearCache();

        void RegisterTable(Type type, string tableName);

        void RegisterFields(Type type, ICollection<IField> fields);
    }
}