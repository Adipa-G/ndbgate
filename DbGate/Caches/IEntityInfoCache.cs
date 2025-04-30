using DbGate.Caches.Impl;
using System;
using System.Collections.Generic;

namespace DbGate.Caches
{
    public interface IEntityInfoCache
    {
        EntityInfo GetEntityInfo(Type entityType);

        EntityInfo GetEntityInfo(IReadOnlyClientEntity entity);

        IList<IRelation> GetReversedRelationships(Type entityType);

        void Register(Type entityType, ITable table, ICollection<IField> fields);

        void Register(Type entityType);

        void Clear();
    }
}