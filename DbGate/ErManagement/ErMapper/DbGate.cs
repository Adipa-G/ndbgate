using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Caches;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.Exceptions.Common;
using log4net;

namespace DbGate.ErManagement.ErMapper
{
    public class DbGate : IDbGate
    {
        private const string DefaultLoggerName = "ER-LAYER";

        private readonly IDbGateConfig config;
        private readonly DataMigrationLayer dataMigrationLayer;
        private readonly IPersistRetrievalLayer persistRetrievalLayer;
        private readonly IDbGateStatistics statistics;

        public DbGate(int dbType)
        {
            config = new DbGateConfig();
            statistics = new DbGateStatistics();
            InitializeDefaults();

            var dbLayer = LayerFactory.CreateLayer(dbType, config);
            CacheManager.Init(config);
            persistRetrievalLayer = new PersistRetrievalLayer(dbLayer, statistics, config);
            dataMigrationLayer = new DataMigrationLayer(dbLayer, statistics, config);
            dbLayer.DataManipulate();
        }

        #region IDbGate Members

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx)
        {
            persistRetrievalLayer.Load(readOnlyEntity, reader, tx);
        }

        public void Save(IEntity entity, ITransaction tx)
        {
            persistRetrievalLayer.Save(entity, tx);
        }

        public ICollection<Object> Select(ISelectionQuery query, ITransaction tx)
        {
            return persistRetrievalLayer.Select(query, tx);
        }

        public void PatchDataBase(ITransaction tx, ICollection<Type> entityTypes, bool dropAll)
        {
            dataMigrationLayer.PatchDataBase(tx, entityTypes, dropAll);
        }

        public void ClearCache()
        {
            persistRetrievalLayer.ClearCache();
        }

        public void RegisterEntity(Type entityType, ITable table, ICollection<IField> fields)
        {
            persistRetrievalLayer.RegisterEntity(entityType, table, fields);
        }

        public IDbGateConfig Config => config;

        public IDbGateStatistics Statistics => statistics;

        #endregion

        private void InitializeDefaults()
        {
            config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            config.LoggerName = DefaultLoggerName;
        }
    }
}