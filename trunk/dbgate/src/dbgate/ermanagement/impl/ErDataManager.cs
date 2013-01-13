using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.query;
using log4net;
using log4net.Repository.Hierarchy;

namespace dbgate.ermanagement.impl
{
    public class ErDataManager : IErDataManager
    {
        private readonly ErDataRetrievalManager _erDataRetrievalManager;
        private readonly ErDataPersistManager _erDataPersistManager;

        public ErDataManager(IDbLayer dbLayer,IErLayerStatistics statistics,IErLayerConfig config)
        {
            _erDataRetrievalManager = new ErDataRetrievalManager(dbLayer,statistics,config);
            _erDataPersistManager = new ErDataPersistManager(dbLayer,statistics,config);
        }

        public void Load(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con) 
        {
            _erDataRetrievalManager.Load(roEntity, reader, con);
        }

        public void Save(IServerDbClass entity,IDbConnection con )
        {
            _erDataPersistManager.Save(entity,con);
        }

        public ICollection<Object> Select(ISelectionQuery query, IDbConnection con)
        {
            return _erDataRetrievalManager.Select(query,con);
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
