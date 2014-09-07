using System;
using System.Collections.Generic;
using System.Data;

namespace DbGate
{
    public interface IDbGate
    {
        IDbGateConfig Config { get; }

        IDbGateStatistics Statistics { get; }

        void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx);

        void Save(IEntity entity, ITransaction tx);

        ICollection<Object> Select(ISelectionQuery query, ITransaction tx);

        void PatchDataBase(ITransaction tx, ICollection<Type> entityTypes, bool dropAll);

        void ClearCache();

        void RegisterEntity(Type entityType, ITable table, ICollection<IField> fields);
    }
}