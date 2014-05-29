using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.exceptions;
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
            String query = CacheManager.QueryCache.GetLoadQuery(targetType);

            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = query;
            ICollection<IDbColumn> keys = CacheManager.FieldCache.GetKeys(targetType);

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
            ITypeFieldValueList valueTypeList = new EntityTypeFieldValueList(type);
            ICollection<IDbColumn> dbColumns = CacheManager.FieldCache.GetDbColumns(type);
            foreach (IDbColumn dbColumn in dbColumns)
            {
                Object value = DbLayer.DataManipulate().ReadFromResultSet(reader, dbColumn);
                valueTypeList.FieldValues.Add(new EntityFieldValue(value, dbColumn));
            }
            return valueTypeList;
        }

        protected static void SetValues(IServerRoDbClass roEntity, ITypeFieldValueList values)
        {
            foreach (EntityFieldValue fieldValue in values.FieldValues)
            {
                PropertyInfo setter = CacheManager.MethodCache.GetProperty(roEntity, fieldValue.DbColumn.AttributeName);
                setter.SetValue(roEntity, fieldValue.Value, null);
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
            ICollection<ITypeFieldValueList> existingEntityChildRelations = new List<ITypeFieldValueList>();

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(parentEntity.GetType(), new[] { typeof(IServerDbClass) });
            foreach (Type type in typeList)
            {
                ICollection<IDbRelation> typeRelations = CacheManager.FieldCache.GetDbRelations(type);
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
                        ErDataManagerUtils.RegisterTypes(childEntity);

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
            }
            return existingEntityChildRelations;
        }

        protected ICollection<IServerRoDbClass> ReadRelationChildrenFromDb(IServerRoDbClass entity, Type type
                , IDbConnection con, IDbRelation relation)
        {
            Type childType = relation.RelatedObjectType;
            IServerRoDbClass childTypeInstance = (IServerRoDbClass)Activator.CreateInstance(childType);
            ErDataManagerUtils.RegisterTypes(childTypeInstance);

            StringBuilder logSb = new StringBuilder();
            String query = CacheManager.QueryCache.GetRelationObjectLoad(entity.GetType(), relation);

            IList<string> fields = new List<string>();
            foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
            {
                fields.Add(mapping.FromField);
            }

            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = query;
            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            ICollection<IDbColumn> dbColumns = CacheManager.FieldCache.GetDbColumns(type);
            for (int i = 0; i < fields.Count; i++)
            {
                String field = fields[i];
                IDbColumn matchColumn = ErDataManagerUtils.FindColumnByAttribute(dbColumns, field);

                if (matchColumn != null)
                {
                    PropertyInfo getter = CacheManager.MethodCache.GetProperty(entity, matchColumn.AttributeName);
                    Object fieldValue = getter.GetValue(entity, null);

                    if (showQuery)
                    {
                        logSb.Append(" ,").Append(matchColumn.ColumnName).Append("=").Append(fieldValue);
                    }
                    DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue, i + 1, matchColumn);
                }
                else
                {
                    String message = String.Format("The field {0} does not have a matching field in the object {1}", field, entity.GetType().FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterSelect(type);
            }
            return ReadFromPreparedStatement(entity, con, cmd, childType);
        }

        private ICollection<IServerRoDbClass> ReadFromPreparedStatement(IServerRoDbClass entity, IDbConnection con, IDbCommand cmd
            , Type childType)
        {
            ICollection<IDbColumn> childKeys = null;
            ICollection<IServerRoDbClass> data = new List<IServerRoDbClass>();
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (childKeys == null)
                {
                    childKeys = CacheManager.FieldCache.GetKeys(childType);
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
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);
            return data;
        }

        protected static bool IsProxyObject(IServerDbClass entity, IDbRelation relation)
        {
            if (relation.Lazy)
            {
                PropertyInfo property = CacheManager.MethodCache.GetProperty(entity,relation.AttributeName);
                Object value = property.GetValue(entity,new object[]{});

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
