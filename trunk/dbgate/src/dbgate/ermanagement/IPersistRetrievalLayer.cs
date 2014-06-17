using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IPersistRetrievalLayer
    {
        void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con);

        void Save(IEntity entity, IDbConnection con);

        ICollection<Object> Select(ISelectionQuery query,IDbConnection con );

        void ClearCache();

        void RegisterEntity(Type entityType, String tableName, ICollection<IField> fields);
    }
}