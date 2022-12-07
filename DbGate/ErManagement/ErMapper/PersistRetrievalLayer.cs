using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Caches;
using DbGate.ErManagement.DbAbstractionLayer;

namespace DbGate.ErManagement.ErMapper
{
    public class PersistRetrievalLayer : IPersistRetrievalLayer
    {
        private readonly RetrievalOperationLayer retrievalOperationLayer;
        private readonly PersistOperationLayer persistOperationLayer;

        public PersistRetrievalLayer(IDbLayer dbLayer,IDbGateStatistics statistics,IDbGateConfig config)
        {
            retrievalOperationLayer = new RetrievalOperationLayer(dbLayer,statistics,config);
            persistOperationLayer = new PersistOperationLayer(dbLayer,statistics,config);
        }

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx) 
        {
            retrievalOperationLayer.Load(readOnlyEntity, reader, tx);
        }

        public void Save(IEntity entity, ITransaction tx)
        {
            persistOperationLayer.Save(entity,tx);
        }

        public ICollection<Object> Select(ISelectionQuery query, ITransaction tx)
        {
            return retrievalOperationLayer.Select(query,tx);
        }

        public void ClearCache()
        {
            CacheManager.Clear();
        }

        public void RegisterEntity(Type entityType, ITable table, ICollection<IField> fields)
        {
            CacheManager.Register(entityType, table, fields);
        }
    }
}
