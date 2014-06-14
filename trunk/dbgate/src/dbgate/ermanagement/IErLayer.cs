using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IErLayer
    {
        void Load(IServerRoDbClass serverRoDbClass, IDataReader reader, IDbConnection con);

        void Save(IServerDbClass serverDbClass, IDbConnection con);

        ICollection<Object> Select(ISelectionQuery query,IDbConnection con );

        void PatchDataBase(IDbConnection con, ICollection<Type> entityTypes, bool dropAll);

        void ClearCache();

        void RegisterEntity(Type entityType, String tableName, ICollection<IField> fields);
        
        IErLayerConfig Config { get; }

        IErLayerStatistics Statistics { get; }
    }
}