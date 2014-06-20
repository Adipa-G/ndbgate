using System;
using System.Collections.Generic;
using System.Data;

namespace dbgate
{
    public interface IDbGate
    {
        void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con);

        void Save(IEntity entity, IDbConnection con);

        ICollection<Object> Select(ISelectionQuery query,IDbConnection con );

        void PatchDataBase(IDbConnection con, ICollection<Type> entityTypes, bool dropAll);

        void ClearCache();

        void RegisterEntity(Type entityType, String tableName, ICollection<IField> fields);
        
        IDbGateConfig Config { get; }

        IDbGateStatistics Statistics { get; }
    }
}