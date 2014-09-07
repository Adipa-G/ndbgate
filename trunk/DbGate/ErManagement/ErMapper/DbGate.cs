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

        private readonly IDbGateConfig _config;
        private readonly DataMigrationLayer _dataMigrationLayer;
        private readonly IPersistRetrievalLayer _persistRetrievalLayer;
        private readonly IDbGateStatistics _statistics;

        public DbGate(int dbType)
        {
            _config = new DbGateConfig();
            _statistics = new DbGateStatistics();
            InitializeDefaults();

            IDbLayer dbLayer = LayerFactory.CreateLayer(dbType, _config);
            CacheManager.Init(_config);
            _persistRetrievalLayer = new PersistRetrievalLayer(dbLayer, _statistics, _config);
            _dataMigrationLayer = new DataMigrationLayer(dbLayer, _statistics, _config);
        }

        #region IDbGate Members

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, ITransaction tx)
        {
            _persistRetrievalLayer.Load(readOnlyEntity, reader, tx);
        }

        public void Save(IEntity entity, ITransaction tx)
        {
            _persistRetrievalLayer.Save(entity, tx);
        }

        public ICollection<Object> Select(ISelectionQuery query, ITransaction tx)
        {
            return _persistRetrievalLayer.Select(query, tx);
        }

        public void PatchDataBase(ITransaction tx, ICollection<Type> entityTypes, bool dropAll)
        {
            _dataMigrationLayer.PatchDataBase(tx, entityTypes, dropAll);
        }

        public void ClearCache()
        {
            _persistRetrievalLayer.ClearCache();
        }

        public void RegisterEntity(Type entityType, ITable table, ICollection<IField> fields)
        {
            _persistRetrievalLayer.RegisterEntity(entityType, table, fields);
        }

        public IDbGateConfig Config
        {
            get { return _config; }
        }

        public IDbGateStatistics Statistics
        {
            get { return _statistics; }
        }

        #endregion

        private void InitializeDefaults()
        {
            _config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            _config.LoggerName = DefaultLoggerName;
        }
    }
}