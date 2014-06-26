using System;
using System.Collections.Generic;
using dbgate.caches.impl;

namespace dbgate.caches
{
    public interface IEntityInfoCache
    {
        EntityInfo GetEntityInfo(Type entityType);

        EntityInfo GetEntityInfo(IReadOnlyClientEntity entity);

        void Register(Type entityType,String tableName,ICollection<IField> fields);

        void Register(Type entityType);
    
        void Clear();
    }
}
