using System;
using System.Collections.Generic;
using DbGate.Caches.Impl;

namespace DbGate.Caches
{
    public interface IEntityInfoCache
    {
        EntityInfo GetEntityInfo(Type entityType);

        EntityInfo GetEntityInfo(IReadOnlyClientEntity entity);

        void Register(Type entityType, String tableName, ICollection<IField> fields);

        void Register(Type entityType);

        void Clear();
    }
}