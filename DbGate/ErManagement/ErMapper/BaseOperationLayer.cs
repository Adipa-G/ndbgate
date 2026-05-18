using Castle.DynamicProxy;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exceptions.Common;
using DbGate.Utility;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DbGate.ErManagement.ErMapper
{
    public abstract class BaseOperationLayer
    {
        private static readonly ConcurrentDictionary<Type, Func<IReadOnlyEntity>> EntityFactoryCache =
            new ConcurrentDictionary<Type, Func<IReadOnlyEntity>>();

        private static readonly ConcurrentDictionary<Type, int[]> ColumnOrdinalsCache =
            new ConcurrentDictionary<Type, int[]>();

        protected IDbLayer DbLayer;
        protected IDbGateStatistics Statistics;
        protected IDbGateConfig Config;

        protected BaseOperationLayer(IDbLayer dbLayer, IDbGateStatistics statistics, IDbGateConfig config)
        {
            DbLayer = dbLayer;
            Statistics = statistics;
            Config = config;
        }

        protected IDbCommand CreateRetrievalPreparedStatement(ITypeFieldValueList keyValueList, ITransaction tx)
        {
            var targetType = keyValueList.Type;
            var entityInfo = CacheManager.GetEntityInfo(targetType);
            var query = entityInfo.GetLoadQuery(DbLayer);

            IDbCommand cmd;
            try
            {
                cmd = tx.CreateCommand();
                cmd.CommandText = query;
            }
            catch (Exception ex)
            {
                var message = String.Format("SQL Exception while trying create command for sql {0}", query);
                throw new CommandCreationException(message, ex);
            }

            var keys = entityInfo.GetKeys();

            var showQuery = Config.ShowQueries;
            StringBuilder logSb = showQuery ? new StringBuilder(query) : null;
            var i = 0;
            foreach (var key in keys)
            {
                var fieldValue = keyValueList.GetFieldValue(key.AttributeName).Value;
                if (showQuery)
                {
                    logSb.Append(" ,").Append(key.ColumnName).Append("=").Append(fieldValue);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue, ++i, key);
            }
            if (showQuery)
            {
                Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterSelect(targetType);
            }
            return cmd;
        }

        protected ITypeFieldValueList ReadValues(Type type, IDataReader reader)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            return ReadValues(entityInfo, reader);
        }

        protected ITypeFieldValueList ReadValues(EntityInfo entityInfo, IDataReader reader)
        {
            var dbColumns = entityInfo.Columns;
            var ordinals = ColumnOrdinalsCache.GetOrAdd(entityInfo.EntityType, _ =>
            {
                var ords = new int[dbColumns.Count];
                var i = 0;
                foreach (var col in dbColumns)
                    ords[i++] = reader.GetOrdinal(col.ColumnName);
                return ords;
            });

            ITypeFieldValueList valueTypeList = new EntityTypeFieldValueList(entityInfo.EntityType);
            var j = 0;
            foreach (var dbColumn in dbColumns)
            {
                var value = DbLayer.DataManipulate().ReadFromResultSet(reader, dbColumn, ordinals[j++]);
                valueTypeList.FieldValues.Add(new EntityFieldValue(value, dbColumn));
            }
            return valueTypeList;
        }

        protected static void SetValues(IReadOnlyEntity roEntity, ITypeFieldValueList values)
        {
            var entityInfo = CacheManager.GetEntityInfo(roEntity);
            SetValues(roEntity, values, entityInfo);
        }

        protected static void SetValues(IReadOnlyEntity roEntity, ITypeFieldValueList values, EntityInfo entityInfo)
        {
            foreach (var fieldValue in values.FieldValues)
            {
                if (entityInfo.FindRelationColumnInfo(fieldValue.Column.AttributeName) == null)
                {
                    var setter = entityInfo.GetProperty(fieldValue.Column.AttributeName);
                    ReflectionUtils.SetValue(entityInfo.EntityType, setter.Name, roEntity, fieldValue.Value);
                }
            }
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueListExcludingDeletedStatusItems(IEntity entity)
        {
            return GetChildEntityValueList(entity, false);
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueListIncludingDeletedStatusItems(IEntity entity)
        {
            return GetChildEntityValueList(entity, true);
        }

        protected static ICollection<ITypeFieldValueList> GetChildEntityValueList(IEntity parentEntity, bool takeDeleted)
        {
            var entityInfo = CacheManager.GetEntityInfo(parentEntity);
            ICollection<ITypeFieldValueList> existingEntityChildRelations = new List<ITypeFieldValueList>();

            while (entityInfo != null)
            {
                var typeRelations = entityInfo.Relations;
                foreach (var typeRelation in typeRelations)
                {
                    if (typeRelation.ReverseRelationship)
                    {
                        continue;
                    }
                    if (IsProxyObject(parentEntity, typeRelation))
                    {
                        continue;
                    }

                    var childEntities = OperationUtils.GetRelationEntities(parentEntity, typeRelation);
                    foreach (var childEntity in childEntities)
                    {
                        if (parentEntity.Status == EntityStatus.Deleted
                            && typeRelation.DeleteRule == ReferentialRuleType.Cascade)
                        {
                            childEntity.Status = EntityStatus.Deleted;
                        }
                        if (childEntity.Status == EntityStatus.Deleted && !takeDeleted)
                        {
                            continue;
                        }
                        var childKeyValueList = OperationUtils.ExtractRelationKeyValues(childEntity, typeRelation);
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

        protected ICollection<IReadOnlyEntity> ReadRelationChildrenFromDb(IReadOnlyEntity entity, Type entityType
                , ITransaction tx, IRelation relation)
        {
            var retrievedEntities = new List<IReadOnlyEntity>();
            var childTypesToProcess = GetChildTypesToProcess(relation);
            var entityInfo = CacheManager.GetEntityInfo(entityType);
            var showQuery = Config.ShowQueries;

            // Pre-extract parent field values once � same mapping fields are used for all child types
            var mappingFields = new List<(IColumn column, object value)>();
            foreach (var mapping in relation.TableColumnMappings)
            {
                var field = mapping.FromField;
                var matchColumn = entityInfo.FindColumnByAttribute(field);
                object fieldValue;
                var entityRelationColumnInfo =
                    entityInfo.FindRelationColumnInfo(matchColumn != null ? matchColumn.AttributeName : "");
                if (entityRelationColumnInfo != null)
                {
                    var entityFieldValue = entity.Context.ChangeTracker.GetFieldValue(matchColumn.AttributeName);
                    fieldValue = entityFieldValue.Value;
                }
                else if (matchColumn != null)
                {
                    var getter = entityInfo.GetProperty(matchColumn.AttributeName);
                    fieldValue = ReflectionUtils.GetValue(entityInfo.EntityType, getter.Name, entity);
                }
                else
                {
                    var message = String.Format("The field {0} does not have a matching field in the object {1}"
                        , field, entity.GetType().FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
                mappingFields.Add((matchColumn, fieldValue));
            }

            var index = 0;
            foreach (var childType in childTypesToProcess)
            {
                index++;
                var effectiveRelation = relation.Clone();
                effectiveRelation.RelatedObjectType = childType;
                effectiveRelation.RelationShipName = relation.RelationShipName + "_" + index;

                var query = entityInfo.GetRelationObjectLoad(DbLayer, effectiveRelation);

                IDbCommand cmd = null;
                try
                {
                    cmd = tx.CreateCommand();
                    cmd.CommandText = query;
                }
                catch (Exception ex)
                {
                    var message = String.Format("SQL Exception while trying create command for sql {0}", query);
                    throw new CommandCreationException(message, ex);
                }

                StringBuilder logSb = showQuery ? new StringBuilder(query) : null;

                for (var i = 0; i < mappingFields.Count; i++)
                {
                    var (matchColumn, fieldValue) = mappingFields[i];
                    if (showQuery)
                    {
                        logSb.Append(" ,").Append(matchColumn.ColumnName).Append("=").Append(fieldValue);
                    }
                    DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue, i + 1, matchColumn);
                }

                if (showQuery)
                {
                    Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
                }
                if (Config.EnableStatistics)
                {
                    Statistics.RegisterSelect(childType);
                }

                var retrievedEntitiesForType = ExecuteAndReadFromPreparedStatement(entity, tx, cmd, childType);
                retrievedEntities.AddRange(retrievedEntitiesForType);
            }
            return retrievedEntities;
        }

        protected ICollection<Type> GetChildTypesToProcess(IRelation relation)
        {
            ICollection<Type> childTypesToProcess = new List<Type>();
            var childType = relation.RelatedObjectType;
            var childEntityInfo = CacheManager.GetEntityInfo(childType);

            if (childEntityInfo.SubEntityInfo.Count > 0)
            {
                foreach (var entityInfo in childEntityInfo.SubEntityInfo)
                {
                    childTypesToProcess.Add(entityInfo.EntityType);
                }
            }
            else
            {
                childTypesToProcess.Add(childType);
            }

            return childTypesToProcess.OrderBy(t => t.FullName).ToList();
        }

        protected ICollection<IReadOnlyEntity> ExecuteAndReadFromPreparedStatement(IReadOnlyEntity entity, ITransaction tx, IDbCommand cmd
            , Type childType)
        {
            var entityInfo = CacheManager.GetEntityInfo(childType);
            var childKeys = entityInfo.GetKeys();
            ICollection<IReadOnlyEntity> data = new List<IReadOnlyEntity>();

            IDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ITypeFieldValueList childTypeKeyList = new EntityTypeFieldValueList(childType);
                    foreach (var childKey in childKeys)
                    {
                        var value = DbLayer.DataManipulate().ReadFromResultSet(reader, childKey);
                        childTypeKeyList.FieldValues.Add(new EntityFieldValue(value, childKey));
                    }
                    if (entity.Context.AlreadyInCurrentObjectGraph(childTypeKeyList))
                    {
                        data.Add(entity.Context.GetFromCurrentObjectGraph(childTypeKeyList));
                        continue;
                    }

                    var factory = EntityFactoryCache.GetOrAdd(childType, t =>
                    {
                        var ctor = t.GetConstructor(Type.EmptyTypes);
                        var newExpr = Expression.New(ctor);
                        return Expression.Lambda<Func<IReadOnlyEntity>>(newExpr).Compile();
                    });
                    var rodbClass = factory();
                    rodbClass.Context.CopyReferenceStoreFrom(entity);
                    rodbClass.Retrieve(reader, tx);
                    data.Add(rodbClass);

                    entity.Context.AddToCurrentObjectGraphIndex(rodbClass);
                }
            }
            catch (Exception ex)
            {
                var message = String.Format("SQL Exception while trying to read type {0} from result set", childType.FullName);
                throw new ReadFromResultSetException(message, ex);
            }
            finally
            {
                DbMgtUtility.Close(reader);
                DbMgtUtility.Close(cmd);
            }

            return data;
        }

        protected static bool IsProxyObject(IEntity entity, IRelation relation)
        {
            if (relation.FetchStrategy == FetchStrategy.Lazy)
            {
                var entityInfo = CacheManager.GetEntityInfo(entity);
                var property = entityInfo.GetProperty(relation.AttributeName);
                var value = ReflectionUtils.GetValue(entityInfo.EntityType, property.Name, entity);

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
