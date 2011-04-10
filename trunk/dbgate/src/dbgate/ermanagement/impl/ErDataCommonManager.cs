using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.utils;
using log4net;

namespace dbgate.ermanagement.impl
{
    public abstract class ErDataCommonManager
    {
        protected IDbLayer DbLayer;
        protected IErLayerConfig Config;

        protected ErDataCommonManager(IDbLayer dbLayer, IErLayerConfig config)
        {
            DbLayer = dbLayer;
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
                DbLayer.GetDataManipulate().SetToPreparedStatement(cmd, fieldValue, ++i, key);
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            return cmd;
        }

        protected ITypeFieldValueList ReadValues(Type type, IDataReader reader)
        {
            ITypeFieldValueList valueTypeList = new EntityTypeFieldValueList(type);
            ICollection<IDbColumn> dbColumns = CacheManager.FieldCache.GetDbColumns(type);
            foreach (IDbColumn dbColumn in dbColumns)
            {
                Object value = DbLayer.GetDataManipulate().ReadFromResultSet(reader, dbColumn);
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
    }
}
