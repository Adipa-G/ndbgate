using System;
using System.Collections.Generic;
using System.Reflection;

namespace dbgate.ermanagement.caches.impl
{
    public class MethodCache : IMethodCache
    {
        private static readonly Dictionary<string, PropertyInfo> Cache = new Dictionary<string, PropertyInfo>();

        #region IMethodCache Members

        public PropertyInfo GetProperty(object obj, string propertyName)
        {
            String cacheKey = CreateCacheKey(obj, propertyName);
            if (Cache.ContainsKey(cacheKey))
            {
                return Cache[cacheKey];
            }

            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            lock (Cache)
            {
                Cache.Add(cacheKey, propertyInfo);
            }
            return propertyInfo;
        }

        public void Clear()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
        }

        #endregion

        public string CreateCacheKey(object obj, string property)
        {
            return obj.GetType().FullName + property;
        }
    }
}