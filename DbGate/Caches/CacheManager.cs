using System;
using System.Collections.Generic;
using DbGate.Caches.Impl;

namespace DbGate.Caches
{
    public class CacheManager
    {
        private static IEntityInfoCache entityInfoCache;

        public static void Init(IDbGateConfig config)
        {
            entityInfoCache = new EntityInfoCache(config);
        }

        public static EntityInfo GetEntityInfo(Type entityType)
        {
            return entityInfoCache.GetEntityInfo(entityType);
        }

        public static EntityInfo GetEntityInfo(IReadOnlyClientEntity entity)
        {
            return entityInfoCache.GetEntityInfo(entity);
        }

        public static IList<IRelation> GetReversedRelationships(Type entityType)
        {
            return entityInfoCache.GetReversedRelationships(entityType);
        }

        public static void Register(Type entityType)
        {
            entityInfoCache.Register(entityType);
        }

        public static void Register(Type entityType, ITable table, ICollection<IField> fields)
        {
            entityInfoCache.Register(entityType, table, fields);
        }

        public static void Clear()
        {
            entityInfoCache.Clear();
        }
    }
}