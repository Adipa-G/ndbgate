using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.exceptions.common;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.query;
using log4net;

namespace dbgate.ermanagement.impl
{
    public class DbGate : IDbGate
    {
        private const string DefaultLoggerName = "ER-LAYER";
    
        private static IDbGate _dbGate;
        private readonly IPersistRetrievalLayer _persistRetrievalLayer;
        private readonly DataMigrationLayer _dataMigrationLayer;
        private readonly IDbGateConfig _config;
        private readonly IDbGateStatistics _statistics;

        private DbGate()
        {
            if (DbConnector.GetSharedInstance() == null)
            {
                throw new DbConnectorNotInitializedException("The DBConnector is not initialized");
            }
            int dbType = DbConnector.GetSharedInstance().DbType;
            _config = new DbGateConfig();
            _statistics = new DbGateStatistics();
            InitializeDefaults();

            IDbLayer dbLayer = LayerFactory.CreateLayer(dbType,_config);
            CacheManager.Init(_config);
            _persistRetrievalLayer = new PersistRetrievalLayer(dbLayer,_statistics,_config);
            _dataMigrationLayer = new DataMigrationLayer(dbLayer,_statistics,_config);
        }

        private void InitializeDefaults()
        {
            _config.AutoTrackChanges = true;
            _config.LoggerName = DefaultLoggerName;
        }

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con)
        {
            _persistRetrievalLayer.Load(readOnlyEntity, reader, con);
        }

        public void Save(IEntity entity, IDbConnection con)
        {
            _persistRetrievalLayer.Save(entity, con);
        }

        public ICollection<Object> Select(ISelectionQuery query,IDbConnection con)
		{
		 	return _persistRetrievalLayer.Select(query,con);
		}

        public void PatchDataBase(IDbConnection con, ICollection<Type> entityTypes, bool dropAll)
        {
            _dataMigrationLayer.PatchDataBase(con, entityTypes, dropAll);
        }

        public void ClearCache()
        {
            _persistRetrievalLayer.ClearCache();
        }

        public void RegisterEntity(Type entityType, string tableName, ICollection<IField> fields)
        {
            _persistRetrievalLayer.RegisterEntity(entityType,tableName,fields);
        }

        public IDbGateConfig Config 
        {
            get { return _config; }
        }

        public IDbGateStatistics Statistics
        {
            get { return _statistics; }
        }

        public static IDbGate GetSharedInstance()
        {
            if (_dbGate == null)
            {
                try
                {
                    _dbGate = new DbGate();
                }
                catch (DbConnectorNotInitializedException e)
                {
                
                    LogManager.GetLogger(DefaultLoggerName).Fatal(e.Message,e);
                }
            }
            return _dbGate;
        }
    }
}
