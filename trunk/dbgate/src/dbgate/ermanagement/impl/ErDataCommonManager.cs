using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.caches.impl;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.exceptions.common;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.utils;
using log4net;

namespace dbgate.ermanagement.impl
{
    public abstract class ErDataCommonManager
    {
        protected IDbLayer DbLayer;
        protected IErLayerStatistics Statistics;
        protected IErLayerConfig Config;

        protected ErDataCommonManager(IDbLayer dbLayer,IErLayerStatistics statistics, IErLayerConfig config)
        {
            DbLayer = dbLayer;
            Statistics = statistics;
            Config = config;
        }

        protected IDbCommand CreateRetrievalPreparedStatement(ITypeFieldValueList keyValueList, IDbConnection con)
        {
            Type targetType = keyValueList.Type;
            EntityInfo entityInfo = CacheManager.GetEntityInfo(targetType);
            string query = entityInfo.GetLoadQuery(DbLayer);

            IDbCommand cmd;
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = query;
            }
            catch (Exception ex)
            {
                string message = String.Format("SQL Exception while trying create command for sql {0}",query);
                throw new CommandCreationException(message,ex);
            }
            
            ICollection<IDbColumn> keys = entityInfo.GetKeys();

            StringBuilder logSb = new StringBuilder();
            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            int i = 0;
            foreach (IDbColumn key in keys)
            {
                Object fieldValue = keyValueList.GetFieldValue(key.AttributeName).Value;
                if (showQuery)
                {
                    logSb.Append(" ,").Append(key.ColumnName).Append("=").Append(fieldValue);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue, ++i, key);
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterSelect(targetType);
            }
            return cmd;
        }

        protected ITypeFieldValueList ReadValues(Type type, IDataReader reader)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);

            ITypeFieldValueList valueTypeList = new EntityTypeFieldValueList(type);
            ICollection<IDbColumn> dbColumns = entityInfo.Columns;
            foreach (IDbColumn dbColumn in dbColumns)
            {
                Object value = DbLayer.DataManipulate().ReadFromResultSet(reader, dbColumn);
                valueTypeList.FieldValues.Add(new EntityFieldValue(value, dbColumn));
            }
            return valueTypeList;
        }

        protected static void SetValues(IServerRoDbClass roEntity, ITypeFieldValueList values)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(roEntity);

            foreach (EntityFieldValue fieldValue in values.FieldValues)
            {
                PropertyInfo setter = entityInfo.GetProperty(fieldValue.DbColumn.AttributeName);
                ReflectionUtils.SetValue(setter, roEntity, fieldValue.Value);
            }
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueListExcludingDeletedStatusItems(IServerDbClass serverDbClass)
        {
            return GetChildEntityValueList(serverDbClass, false);
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueListIncludingDeletedStatusItems(IServerDbClass serverDbClass)
        {
            return GetChildEntityValueList(serverDbClass, true);
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueList(IServerDbClass parentEntity, bool takeDeleted)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(parentEntity);
            ICollection<ITypeFieldValueList> existingEntityChildRelations = new List<ITypeFieldValueList>();

            while (entityInfo != null)
            {
                ICollection<IDbRelation> typeRelations = entityInfo.Relations;
                foreach (IDbRelation typeRelation in typeRelations)
                {
                    if (typeRelation.ReverseRelationship)
                    {
                        continue;
                    }
                    if (IsProxyObject(parentEntity,typeRelation))
                    {
                        continue;
                    }

                    ICollection<IServerDbClass> childEntities = ErDataManagerUtils.GetRelationEntities(parentEntity, typeRelation);
                    foreach (IServerDbClass childEntity in childEntities)
                    {
                        if (parentEntity.Status == DbClassStatus.Deleted
                            && typeRelation.DeleteRule == ReferentialRuleType.Cascade)
                        {
                            childEntity.Status = DbClassStatus.Deleted;
                        }
                        if (childEntity.Status == DbClassStatus.Deleted && !takeDeleted)
                        {
                            continue;
                        }
                        ITypeFieldValueList childKeyValueList = ErDataManagerUtils.ExtractRelationKeyValues(childEntity, typeRelation);
                        if (childKeyValueList != null)
                        {
                            existingEntityChildRelations.Add(childKeyValueList);
                        }
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
            return existingEntityChildRelations;
        }

        protected ICollection<IServerRoDbClass> ReadRelationChildrenFromDb(IServerRoDbClass entity, Type entityType
                , IDbConnection con, IDbRelation relation)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);
            Type childEntityType = relation.RelatedObjectType;
            IServerRoDbClass childTypeInstance = (IServerRoDbClass)Activator.CreateInstance(childEntityType);

            StringBuilder logSb = new StringBuilder();
            string query = entityInfo.GetRelationObjectLoad(DbLayer, relation);

            IList<string> fields = new List<string>();
            foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
            {
                fields.Add(mapping.FromField);
            }

            IDbCommand cmd = null;
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = query;
            }
            catch (Exception ex)
            {
                string message = String.Format("SQL Exception while trying create command for sql {0}",query);
                throw new CommandCreationException(message,ex);  
            }
            
            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            ICollection<IDbColumn> dbColumns = entityInfo.Columns;
            for (int i = 0; i < fields.Count; i++)
            {
                string field = fields[i];
                IDbColumn matchColumn = ErDataManagerUtils.FindColumnByAttribute(dbColumns, field);

                if (matchColumn != null)
                {
                    PropertyInfo getter = entityInfo.GetProperty(matchColumn.AttributeName);
                    Object fieldValue = ReflectionUtils.GetValue(getter, entity);
                    
                    if (showQuery)
                    {
                        logSb.Append(" ,").Append(matchColumn.ColumnName).Append("=").Append(fieldValue);
                    }
                    DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue, i + 1, matchColumn);
                }
                else
                {
                    string message = String.Format("The field {0} does not have a matching field in the object {1}", field, entity.GetType().FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterSelect(entityType);
            }
            return ExecuteAndReadFromPreparedStatement(entity, con, cmd, childEntityType);
        }

        private ICollection<IServerRoDbClass> ExecuteAndReadFromPreparedStatement(IServerRoDbClass entity, IDbConnection con, IDbCommand cmd
            , Type childType)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(childType);
            ICollection<IDbColumn> childKeys = null;
            ICollection<IServerRoDbClass> data = new List<IServerRoDbClass>();

            IDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (childKeys == null)
                    {
                        childKeys = entityInfo.GetKeys();
                    }
                    ITypeFieldValueList childTypeKeyList = new EntityTypeFieldValueList(childType);
                    foreach (IDbColumn childKey in childKeys)
                    {
                        Object value = DbLayer.DataManipulate().ReadFromResultSet(reader, childKey);
                        childTypeKeyList.FieldValues.Add(new EntityFieldValue(value, childKey));
                    }
                    if (ErSessionUtils.ExistsInSession(entity, childTypeKeyList))
                    {
                        data.Add(ErSessionUtils.GetFromSession(entity, childTypeKeyList));
                        continue;
                    }

                    IServerRoDbClass rodbClass = (IServerRoDbClass)Activator.CreateInstance(childType);
                    ErSessionUtils.TransferSession(entity, rodbClass);
                    rodbClass.Retrieve(reader, con);
                    data.Add(rodbClass);

                    IEntityFieldValueList childEntityKeyList = ErDataManagerUtils.ExtractEntityKeyValues(rodbClass);
                    ErSessionUtils.AddToSession(entity, childEntityKeyList);
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("SQL Exception while trying to read type {0} from result set",childType.FullName);
                throw new ReadFromResultSetException(message,ex);
            }
            finally
            {
                DbMgmtUtility.Close(reader);
                DbMgmtUtility.Close(cmd);     
            }

            return data;
        }

        protected static bool IsProxyObject(IServerDbClass entity, IDbRelation relation)
        {
            if (relation.Lazy)
            {
                EntityInfo entityInfo = CacheManager.GetEntityInfo(entity);
                PropertyInfo property = entityInfo.GetProperty(relation.AttributeName);
                Object value = ReflectionUtils.GetValue(property, entity); ;
               
                if (value == null)
                {
                    return false;
                }

                if (ProxyUtil.IsProxyType(value.GetType()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
