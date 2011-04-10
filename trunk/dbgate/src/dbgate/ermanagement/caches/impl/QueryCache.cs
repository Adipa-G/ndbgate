using System;
using System.Collections.Generic;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;

namespace dbgate.ermanagement.caches.impl
{
    public class QueryCache : IQueryCache
    {
        private const string Insert = "INS";
        private const string Update = "UPD";
        private const string Delete = "DEL";
        private const string Load = "LOD";

        private static readonly object LockObj = new object();

        private static readonly Dictionary<string, QueryHolder> QueryMap = new Dictionary<string, QueryHolder>();
        private readonly IDbLayer _dbLayer;

        public QueryCache(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }

        private static string CreateCacheKey(string tableName, Type type)
        {
            return tableName + "_" + type.FullName;
        }

        private static QueryHolder GetHolder(string key)
        {
            if (!QueryMap.ContainsKey(key))
            {
                lock (LockObj)
                {
                    QueryMap.Add(key, new QueryHolder());
                }
            }
            return QueryMap[key];
        }

        private static string GetQuery(string tableName, Type type, string id)
        {
            QueryHolder holder = GetHolder(CreateCacheKey(tableName, type));
            return holder.GetQuery(id);
        }

        private static void SetQuery(string tableName, Type type, string id, string query)
        {
            QueryHolder holder = GetHolder(CreateCacheKey(tableName, type));
            holder.SetQuery(id, query);
        }

        #region IQueryCache Members
        public string GetLoadQuery(Type entityType)
        {
            string tableName = CacheManager.TableCache.GetTableName(entityType);
            string query = GetQuery(tableName, entityType, Load);
            if (query == null)
            {
                query = _dbLayer.GetDataManipulate().CreateLoadQuery(tableName, CacheManager.FieldCache.GetDbColumns(entityType));
                SetQuery(tableName, entityType, Load, query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Load Query building failed for table {0} class {1}",tableName,entityType.FullName));
            }
            return query;
        }

        public string GetInsertQuery(Type entityType)
        {
            string tableName = CacheManager.TableCache.GetTableName(entityType);
            string query = GetQuery(tableName, entityType, Insert);
            if (query == null)
            {
                query = _dbLayer.GetDataManipulate().CreateInsertQuery(tableName, CacheManager.FieldCache.GetDbColumns(entityType));
                SetQuery(tableName, entityType, Insert, query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Insert Query building failed for table {0} class {1}", tableName, entityType.FullName));
            }
            return query;
        }

        public string GetUpdateQuery(Type entityType)
        {
            string tableName = CacheManager.TableCache.GetTableName(entityType);
            string query = GetQuery(tableName, entityType, Update);
            if (query == null)
            {
                query = _dbLayer.GetDataManipulate().CreateUpdateQuery(tableName, CacheManager.FieldCache.GetDbColumns(entityType));
                SetQuery(tableName, entityType, Update, query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Update Query building failed for table {0} class {1}", tableName, entityType.FullName));
            }
            return query;
        }

        public string GetDeleteQuery(Type entityType)
        {
            string tableName = CacheManager.TableCache.GetTableName(entityType);
            string query = GetQuery(tableName, entityType, Delete);
            if (query == null)
            {
                query = _dbLayer.GetDataManipulate().CreateDeleteQuery(tableName, CacheManager.FieldCache.GetDbColumns(entityType));
                SetQuery(tableName, entityType, Delete, query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Delete Query building failed for table {0} class {1}", tableName, entityType.FullName));
            }
            return query;
        }

        public string GetRelationObjectLoad(Type entityType, IDbRelation relation)
        {
            string tableName = CacheManager.TableCache.GetTableName(entityType);
            string query = GetQuery(tableName, entityType, relation.RelationShipName);
            if (query == null)
            {
                query = _dbLayer.GetDataManipulate().CreateRelatedObjectsLoadQuery(tableName, relation);
                SetQuery(tableName, entityType, relation.RelationShipName, query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Relation object load Query building failed for table {0} class {1}", tableName, entityType.FullName));
            }
            return query;
        }

        public void Clear()
        {
            QueryMap.Clear();
        }
        #endregion
    }
}