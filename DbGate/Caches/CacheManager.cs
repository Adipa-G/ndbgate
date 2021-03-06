using System;
using System.Collections.Generic;
using DbGate.Caches.Impl;

namespace DbGate.Caches
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

        public static IList<IRelation> GetReversedRelationships(Type entityType)
        {
            return _entityInfoCache.GetReversedRelationships(entityType);
        }

        public static void Register(Type entityType)
        {
            _entityInfoCache.Register(entityType);
        }

        public static void Register(Type entityType, ITable table, ICollection<IField> fields)
        {
            _entityInfoCache.Register(entityType, table, fields);
        }

        public static void Clear()
        {
            _entityInfoCache.Clear();
        }
    }
}