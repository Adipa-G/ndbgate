using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Caches;
using DbGate.ErManagement.DbAbstractionLayer;

namespace DbGate.ErManagement.ErMapper
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
