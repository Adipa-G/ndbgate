using System;
using System.Collections.Generic;
using System.Data;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;
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
                throw new DBConnectorNotInitializedException("The DBConnector is not initialized");
            }
            int dbType = DbConnector.GetSharedInstance().DbType;
            _config = new ErLayerConfig();
            _statistics = new ErLayerStatistics();
            InitializeDefaults();

            IDbLayer dbLayer = LayerFactory.CreateLayer(dbType,_config);
            CacheManager.Init(dbLayer);
            _erDataManager = new ErDataManager(dbLayer,_statistics,_config);
            _erMetaDataManager = new ErMetaDataManager(dbLayer,_statistics,_config);
        }

        private void InitializeDefaults()
        {
            _config.AutoTrackChanges = true;
            _config.LoggerName = DefaultLoggerName;
        }

        public void Load(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con)
        {
            _erDataManager.Load(roEntity, reader, con);
        }

        public void Save(IServerDbClass serverDbClass, IDbConnection con)
        {
            _erDataManager.Save(serverDbClass, con);
        }

        public void PatchDataBase(IDbConnection con, ICollection<IServerDbClass> dbClasses, bool dropAll)
        {
            _erMetaDataManager.PatchDataBase(con, dbClasses, dropAll);
        }

        public void ClearCache()
        {
            _erDataManager.ClearCache();
        }

        public void RegisterTable(Type type, string tableName)
        {
            _erDataManager.RegisterTable(type,tableName);
        }

        public void RegisterFields(Type type, ICollection<IField> fields)
        {
            _erDataManager.RegisterFields(type,fields);
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
                catch (DBConnectorNotInitializedException e)
                {
                
                    LogManager.GetLogger(DefaultLoggerName).Fatal(e.Message,e);
                }
            }
            return _erLayer;
        }
    }
}
