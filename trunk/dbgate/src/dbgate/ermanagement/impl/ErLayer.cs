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
    public class ErLayer : IErLayer
    {
        private const string DefaultLoggerName = "ER-LAYER";
    
        private static IErLayer _erLayer;
        private readonly IErDataManager _erDataManager;
        private readonly ErMetaDataManager _erMetaDataManager;
        private readonly IErLayerConfig _config;
        private readonly IErLayerStatistics _statistics;

        private ErLayer()
        {
            if (DbConnector.GetSharedInstance() == null)
            {
                throw new DbConnectorNotInitializedException("The DBConnector is not initialized");
            }
            int dbType = DbConnector.GetSharedInstance().DbType;
            _config = new ErLayerConfig();
            _statistics = new ErLayerStatistics();
            InitializeDefaults();

            IDbLayer dbLayer = LayerFactory.CreateLayer(dbType,_config);
            CacheManager.Init(_config);
            _erDataManager = new ErDataManager(dbLayer,_statistics,_config);
            _erMetaDataManager = new ErMetaDataManager(dbLayer,_statistics,_config);
        }

        private void InitializeDefaults()
        {
            _config.AutoTrackChanges = true;
            _config.LoggerName = DefaultLoggerName;
        }

        public void Load(IReadOnlyEntity readOnlyEntity, IDataReader reader, IDbConnection con)
        {
            _erDataManager.Load(readOnlyEntity, reader, con);
        }

        public void Save(IEntity entity, IDbConnection con)
        {
            _erDataManager.Save(entity, con);
        }

        public ICollection<Object> Select(ISelectionQuery query,IDbConnection con)
		{
		 	return _erDataManager.Select(query,con);
		}

        public void PatchDataBase(IDbConnection con, ICollection<Type> entityTypes, bool dropAll)
        {
            _erMetaDataManager.PatchDataBase(con, entityTypes, dropAll);
        }

        public void ClearCache()
        {
            _erDataManager.ClearCache();
        }

        public void RegisterEntity(Type entityType, string tableName, ICollection<IField> fields)
        {
            _erDataManager.RegisterEntity(entityType,tableName,fields);
        }

        public IErLayerConfig Config 
        {
            get { return _config; }
        }

        public IErLayerStatistics Statistics
        {
            get { return _statistics; }
        }

        public static IErLayer GetSharedInstance()
        {
            if (_erLayer == null)
            {
                try
                {
                    _erLayer = new ErLayer();
                }
                catch (DbConnectorNotInitializedException e)
                {
                
                    LogManager.GetLogger(DefaultLoggerName).Fatal(e.Message,e);
                }
            }
            return _erLayer;
        }
    }
}
