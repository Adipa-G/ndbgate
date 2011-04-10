using dbgate.ermanagement.caches.impl;
using dbgate.ermanagement.impl.dbabstractionlayer;

namespace dbgate.ermanagement.caches
{
    public class CacheManager
    {
        public static IQueryCache QueryCache;
        public static IFieldCache FieldCache;
        public static IMethodCache MethodCache;
        public static ITableCache TableCache;

        public static void Init(IDbLayer dbLayer)
        {
            QueryCache = new QueryCache(dbLayer);
            FieldCache = new FieldCache();
            MethodCache = new MethodCache();
            TableCache = new TableCache();
        }
    }
}
