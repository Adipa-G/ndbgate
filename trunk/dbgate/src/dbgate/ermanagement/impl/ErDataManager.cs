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
        private IDbLayer _dbLayer;
		private IErLayerConfig _config;

        public ErDataManager(IDbLayer dbLayer,IErLayerStatistics statistics,IErLayerConfig config)
        {
            _dbLayer = dbLayer;
            _config = config;
            _erDataRetrievalManager = new ErDataRetrievalManager(dbLayer,statistics,config);
            _erDataPersistManager = new ErDataPersistManager(dbLayer,statistics,config);
        }

        public void Load(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con) 
        {
            _erDataRetrievalManager.Load(roEntity, reader, con);
        }

        public void Save(IServerDbClass entity,IDbConnection con )
        {
            _erDataPersistManager.Save(entity,con);
        }

        public ICollection<Object> Select(ISelectionQuery query, IDbConnection con)
        {
            IDataReader reader = null;
            try
            {
                StringBuilder logSb = new StringBuilder();
                bool showQuery = _config.ShowQueries;
                QueryExecInfo execInfo = _dbLayer.GetDataManipulate().CreateExecInfo(con, query);
                if (showQuery)
                {
                    logSb.Append(execInfo.Sql);
                    foreach (QueryParam param in execInfo.Params)
                    {
                        logSb.Append(" ,").Append("Param").Append(param.Index).Append("=").Append(param.Value);
                    }
                    LogManager.GetLogger(_config.LoggerName).Info(logSb.ToString());
                }

                reader = _dbLayer.GetDataManipulate().CreateResultSet(con, execInfo);
                
                ICollection<Object> retList = new List<Object>();
                ICollection<IQuerySelection> selections = query.Structure.SelectList;

                while (reader.Read())
                {
                    int count = 0;
                    Object[] rowObjects = new Object[selections.Count];
                    foreach (IQuerySelection selection in selections)
                    {
                        Object loaded = selection.Retrieve(reader);
                        rowObjects[count++] = loaded;
                    }
                    retList.Add(rowObjects);
                }

                return retList;
            } 
            catch (Exception e)
            {
                LogManager.GetLogger(_config.LoggerName).Fatal(e.Message, e);
                throw new RetrievalException(e.Message,e);
            } 
            finally
            {
                DbMgmtUtility.Close(reader);
            }
        }

        public void ClearCache()
        {
            CacheManager.FieldCache.Clear();
            CacheManager.MethodCache.Clear();
            CacheManager.TableCache.Clear();
            CacheManager.QueryCache.Clear();
        }

        public void RegisterTable(Type type, string tableName)
        {
            CacheManager.TableCache.Register(type,tableName);
        }

        public void RegisterFields(Type type, ICollection<IField> fields)
        {
            CacheManager.FieldCache.Register(type,fields);
        }
    }
}
