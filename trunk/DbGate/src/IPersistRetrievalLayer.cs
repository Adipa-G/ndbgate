using System;
using System.Collections.Generic;
using System.Data;

namespace DbGate
{
    public interface IPersistRetrievalLayer
    {
        void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx);

        void Save(IEntity entity, ITransaction tx);

        ICollection<Object> Select(ISelectionQuery query, ITransaction tx);

        void ClearCache();

        void RegisterEntity(Type entityType, ITable table, ICollection<IField> fields);
    }
}