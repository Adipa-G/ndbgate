using System;
using System.Collections.Generic;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.caches.impl
{
    public class TableCache : ITableCache
    {
        private static readonly IDictionary<Type, string> Cache = new Dictionary<Type, string>();

        #region ITableCache Members

        public String GetTableName(Type type)
        {
            if (Cache.ContainsKey(type))
            {
                return Cache[type];
            }
            throw new TableCacheMissException(String.Format("No cache entry found for {0}", type.FullName));
        }

        public void Register(Type type, string tableName)
        {
            if (Cache.ContainsKey(type))
            {
                Cache.Remove(type);
            }
            Cache.Add(type, tableName);
        }

        public void Register(Type type, IServerRoDbClass serverRoDbClass)
        {
            String tableName = DbClassAttributeExtractionUtils.GetTableName(serverRoDbClass, type);
            if (Cache.ContainsKey(type))
            {
                return;
            }
            lock (Cache)
            {
                Cache.Add(type, tableName);
            }
        }

        public void Clear()
        {
            Cache.Clear();
        }

        #endregion
    }
}