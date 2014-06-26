using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using dbgate.caches;
using dbgate.dbutility;
using dbgate.ermanagement.dbabstractionlayer;
using dbgate.ermanagement.query;
using log4net;
using log4net.Repository.Hierarchy;

namespace dbgate.ermanagement.impl
{
    public class PersistRetrievalLayer : IPersistRetrievalLayer
    {
        private readonly RetrievalOperationLayer _retrievalOperationLayer;
        private readonly PersistOperationLayer _persistOperationLayer;

        public PersistRetrievalLayer(IDbLayer dbLayer,IDbGateStatistics statistics,IDbGateConfig config)
        {
            _retrievalOperationLayer = new RetrievalOperationLayer(dbLayer,statistics,config);
            _persistOperationLayer = new PersistOperationLayer(dbLayer,statistics,config);
        }

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con) 
        {
            _retrievalOperationLayer.Load(readOnlyEntity, reader, con);
        }

        public void Save(IEntity entity,IDbConnection con )
        {
            _persistOperationLayer.Save(entity,con);
        }

        public ICollection<Object> Select(ISelectionQuery query, IDbConnection con)
        {
            return _retrievalOperationLayer.Select(query,con);
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
