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

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con) 
        {
            _erDataRetrievalManager.Load(readOnlyEntity, reader, con);
        }

        public void Save(IEntity entity,IDbConnection con )
        {
            _erDataPersistManager.Save(entity,con);
        }

        public ICollection<Object> Select(ISelectionQuery query, IDbConnection con)
        {
            return _erDataRetrievalManager.Select(query,con);
        }

        public void ClearCache()
        {
            CacheManager.Clear();
        }

        public void RegisterEntity(Type entityType, string tableName, ICollection<IField> fields)
        {
            CacheManager.Register(entityType,tableName,fields);
        }
    }
}
