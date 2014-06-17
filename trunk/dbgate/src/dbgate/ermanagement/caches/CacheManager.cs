using System;
using System.Collections.Generic;
using dbgate.ermanagement.caches.impl;
using dbgate.ermanagement.impl.dbabstractionlayer;

namespace dbgate.ermanagement.caches
{
    public class CacheManager
    {
        private static IEntityInfoCache _entityInfoCache;

        public static void Init(IDbGateConfig config)
        {
            _entityInfoCache = new EntityInfoCache(config);
        }

        public static EntityInfo GetEntityInfo(Type entityType)
        {
            return _entityInfoCache.GetEntityInfo(entityType);
        }

        public static EntityInfo GetEntityInfo(IReadOnlyClientEntity entity)
        {
            return _entityInfoCache.GetEntityInfo(entity);
        }

        public static void Register(Type entityType)
        {
            _entityInfoCache.Register(entityType);
        }

        public static void Register(Type entityType,String tableName,ICollection<IField> fields)
        {
            _entityInfoCache.Register(entityType,tableName,fields);
        }

        public static void Clear()
        {
            _entityInfoCache.Clear();
        }
    }
}
