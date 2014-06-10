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

        public string GetTableName(Type entityType)
        {
            if (Cache.ContainsKey(entityType))
            {
                return Cache[entityType];
            }
            throw new TableCacheMissException(String.Format("No cache entry found for {0}", entityType.FullName));
        }

        public void Register(Type entityType, string tableName)
        {
            if (Cache.ContainsKey(entityType))
            {
                return;    
            }
            lock (Cache)
            {
                Cache.Add(entityType, tableName);    
            }
        }

        public void Register(Type entityType)
        {
            if (Cache.ContainsKey(entityType))
            {
                return;
            }

            IManagedDbClass managedDbClass = null;
            if (ReflectionUtils.IsImplementInterface(entityType,typeof(IManagedDbClass)))
            {
                try
                {
                    managedDbClass = (IManagedDbClass)Activator.CreateInstance(entityType);
                }
                catch (Exception e)
                {
                    throw  new EntityRegistrationException(String.Format("Could not register type {0}",entityType.FullName),e);
                }
            }

            var tempStore = new Dictionary<Type, string>();
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(entityType,new[]{typeof(IServerRoDbClass)});
            foreach (Type regType in typeList)
            {
                if (Cache.ContainsKey(regType))
                {
                    continue;
                }
                String tableName = managedDbClass != null && managedDbClass.TableNames.ContainsKey(regType)
                        ? managedDbClass.TableNames[regType]
                        : DbClassAttributeExtractionUtils.GetTableName(regType);
                tempStore.Add(regType,tableName);
            }

            lock (Cache)
            {
                foreach (Type type in tempStore.Keys)
                {
                    Cache.Add(type, tempStore[type]);
                }
            }
        }

        public void Clear()
        {
            Cache.Clear();
        }

        #endregion
    }
}