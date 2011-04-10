using System;
using System.Collections.Generic;
using System.Data;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.impl.dbabstractionlayer;

namespace dbgate.ermanagement.impl
{
    public class ErDataManager : IErDataManager
    {
        private readonly ErDataRetrievalManager _erDataRetrievalManager;
        private readonly ErDataPersistManager _erDataPersistManager;

        public ErDataManager(IDbLayer dbLayer,IErLayerConfig config)
        {
            _erDataRetrievalManager = new ErDataRetrievalManager(dbLayer,config);
            _erDataPersistManager = new ErDataPersistManager(dbLayer,config);
        }

        public void Load(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con) 
        {
            _erDataRetrievalManager.Load(roEntity, reader, con);
        }

        public void Save(IServerDbClass entity,IDbConnection con )
        {
            _erDataPersistManager.Save(entity,con);
        }

        public void ClearCache()
        {
            CacheManager.FieldCache.Clear();
            CacheManager.MethodCache.Clear();
            CacheManager.TableCache.Clear();
            CacheManager.QueryCache.Clear();
        }

        public void RegisterTable(Type type, string tableName)
        {
            CacheManager.TableCache.Register(type,tableName);
        }

        public void RegisterFields(Type type, ICollection<IField> fields)
        {
            CacheManager.FieldCache.Register(type,fields);
        }
    }
}
