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

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx) 
        {
            _retrievalOperationLayer.Load(readOnlyEntity, reader, tx);
        }

        public void Save(IEntity entity, ITransaction tx)
        {
            _persistOperationLayer.Save(entity,tx);
        }

        public ICollection<Object> Select(ISelectionQuery query, ITransaction tx)
        {
            return _retrievalOperationLayer.Select(query,tx);
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
