using System;
using System.Collections.Generic;
using System.Data;

namespace dbgate.ermanagement
{
    public interface IErLayer
    {
        void Load(IServerRoDbClass serverRoDbClass, IDataReader reader, IDbConnection con);

        void Save(IServerDbClass serverDbClass, IDbConnection con);

        void PatchDataBase(IDbConnection con, ICollection<IServerDbClass> dbClasses, bool dropAll);

        void ClearCache();

        void RegisterTable(Type type, string tableName);

        void RegisterFields(Type type, ICollection<IField> fields);

        IErLayerConfig Config { get; }

        IErLayerStatistics Statistics { get; }
    }
}