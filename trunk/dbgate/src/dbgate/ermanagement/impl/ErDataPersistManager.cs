using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Text;
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
    public class ErDataPersistManager : ErDataCommonManager
    {
        public ErDataPersistManager(IDbLayer dbLayer,IErLayerStatistics statistics, IErLayerConfig config)
            : base(dbLayer,statistics, config)
        {
        }

        public void Save(IServerDbClass entity, IDbConnection con)
        {
            try
            {
                ErSessionUtils.InitSession(entity);
                ErDataManagerUtils.RegisterTypes(entity);
                TrackAndCommitChanges(entity, con);

                Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(entity.GetType(), new[] { typeof(IServerDbClass) });
                ErDataManagerUtils.Reverse(typeList); //use reverse order not to break the super to sub constraints
                foreach (Type type in typeList)
                {
                    SaveForType(entity, type, con);
                }
                entity.Status = DbClassStatus.Unmodified;
                ErSessionUtils.DestroySession(entity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(e.Message, e);
                throw new PersistException(e.Message, e);
            }
        }

        private void TrackAndCommitChanges(IServerDbClass serverDBClass, IDbConnection con)
        {
            IEntityContext entityContext = serverDBClass.Context;
            ICollection<ITypeFieldValueList> originalChildren;

            if (entityContext != null)
            {
                if (Config.AutoTrackChanges)
                {
                    if (CheckForModification(serverDBClass,con, entityContext))
                    {
                        MiscUtils.Modify(serverDBClass);
                    }
                }
                originalChildren = entityContext.ChangeTracker.ChildEntityKeys;
            }
            else
            {
                originalChildren = GetChildEntityValueListIncludingDeletedStatusItems(serverDBClass);
            }

            ICollection<ITypeFieldValueList> currentChildren = GetChildEntityValueListExcludingDeletedStatusItems(serverDBClass);
            ValidateForChildDeletion(serverDBClass, currentChildren);
            ICollection<ITypeFieldValueList> deletedChildren = ErDataManagerUtils.FindDeletedChildren(originalChildren, currentChildren);
            DeleteOrphanChildren(con, deletedChildren);
        }

        private void SaveForType(IServerDbClass entity, Type type, IDbConnection con)
        {
            String tableName = CacheManager.TableCache.GetTableName(type);
            if (tableName == null)
            {
                return;
            }

            ICollection<IDbRelation> dbRelations = CacheManager.FieldCache.GetDbRelations(type);
            foreach (IDbRelation relation in dbRelations)
            {
                if (relation.ReverseRelationship)
                {
                    continue;
                }
                if (IsProxyObject(entity, relation))
                {
                    continue;
                }
                ICollection<IServerDbClass> childObjects = ErDataManagerUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {
                    if (relation.NonIdentifyingRelation)
                    {
                        foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
                        {
                            SetParentRelationFieldsForNonIdentifyingRelations(entity, childObjects, mapping);
                        }
                    }
                }
            }

            ITypeFieldValueList fieldValues = ErDataManagerUtils.ExtractTypeFieldValues(entity, type);
            if (entity.Status == DbClassStatus.Unmodified)
            {
                //do nothing
            }
            else if (entity.Status == DbClassStatus.New)
            {
                Insert(entity,fieldValues, type, con);
            }
            else if (entity.Status == DbClassStatus.Modified)
            {
                if (Config.CheckVersion)
                {
                    if (!VersionValidated(entity, type, con))
                    {
                        throw new DataUpdatedFromAnotherSourceException(String.Format("The type {0} updated from another transaction", type));
                    }
                    ErDataManagerUtils.IncrementVersion(fieldValues);
                    SetValues(entity, fieldValues);
                }
                Update(fieldValues, type, con);
            }
            else if (entity.Status == DbClassStatus.Deleted)
            {
                Delete(fieldValues, type, con);
            }
            else
            {
                String message = String.Format("In-corret status for class {0}", type.FullName);
                throw new IncorrectStatusException(message);
            }

            fieldValues = ErDataManagerUtils.ExtractTypeFieldValues(entity, type);
            IEntityContext entityContext = entity.Context;
            if (entityContext != null)
            {
                foreach (EntityFieldValue fieldValue in fieldValues.FieldValues)
                {
                    EntityFieldValue entityFieldValue = entityContext.ChangeTracker.GetFieldValue(fieldValue.DbColumn.AttributeName);
                    if (entityFieldValue == null)
                    {
                        entityContext.ChangeTracker.Fields.Add(fieldValue);
                    }
                    else
                    {
                        entityFieldValue.Value = fieldValue.Value;
                    }
                }
            }
            ErSessionUtils.AddToSession(entity, ErDataManagerUtils.ExtractEntityKeyValues(entity));

            foreach (IDbRelation relation in dbRelations)
            {
                if (relation.ReverseRelationship
                        || relation.NonIdentifyingRelation)
                {
                    continue;
                }
                if (IsProxyObject(entity, relation))
                {
                    continue;
                }

                ICollection<IServerDbClass> childObjects = ErDataManagerUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {

                    SetRelationObjectKeyValues(fieldValues, type, childObjects, relation);
                    foreach (IServerDbClass fieldObject in childObjects)
                    {
                        IEntityFieldValueList childEntityKeyList = ErDataManagerUtils.ExtractEntityKeyValues(fieldObject);
                        if (ErSessionUtils.ExistsInSession(entity, childEntityKeyList))
                        {
                            continue;
                        }
                        ErSessionUtils.TransferSession(entity, fieldObject);
                        if (fieldObject.Status != DbClassStatus.Deleted) //deleted items are already deleted
                        {
                            fieldObject.Persist(con);
                            ErSessionUtils.AddToSession(entity, childEntityKeyList);
                        }
                    }
                }
            }
        }

        private void Insert(IServerDbClass entity,ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
        {
            StringBuilder logSb = new StringBuilder();
            String query = CacheManager.QueryCache.GetInsertQuery(type);
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = query;

            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            int i = 0;
            foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
            {
                IDbColumn dbColumn = fieldValue.DbColumn;
                Object columnValue;

                if (dbColumn.ReadFromSequence
                        && dbColumn.SequenceGenerator != null)
                {
                    columnValue = dbColumn.SequenceGenerator.GetNextSequenceValue(con);
                    PropertyInfo setter = CacheManager.MethodCache.GetProperty(entity, dbColumn.AttributeName);
                    setter.SetValue(entity,columnValue,null);
                }
                else
                {
                    columnValue = fieldValue.Value;
                }
                if (showQuery)
                {
                    logSb.Append(" ,").Append(dbColumn.ColumnName).Append("=").Append(columnValue);
                }
                DbLayer.GetDataManipulate().SetToPreparedStatement(cmd, columnValue, i + 1, dbColumn);
                i++;
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterInsert(type);
            }
            cmd.ExecuteNonQuery();
            DbMgmtUtility.Close(cmd);
        }

        private void Update(ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
        {
            StringBuilder logSb = new StringBuilder();
            String query = CacheManager.QueryCache.GetUpdateQuery(type);

            IList<EntityFieldValue> keys = new List<EntityFieldValue>();
            IList<EntityFieldValue> values = new List<EntityFieldValue>();
            foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
            {
                if (fieldValue.DbColumn.Key)
                {
                    keys.Add(fieldValue);
                }
                else
                {
                    values.Add(fieldValue);
                }
            }

            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }

            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = query;
            int count = 0;
            foreach (EntityFieldValue fieldValue in values)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.DbColumn.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.GetDataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.DbColumn);
            }

            foreach (EntityFieldValue fieldValue in keys)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.DbColumn.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.GetDataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.DbColumn);
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterUpdate(type);
            }
            cmd.ExecuteNonQuery();
            DbMgmtUtility.Close(cmd);
        }

        private void Delete(ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
        {
            StringBuilder logSb = new StringBuilder();
            String query = CacheManager.QueryCache.GetDeleteQuery(type);
            IList<EntityFieldValue> keys = new List<EntityFieldValue>();

            foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
            {
                if (fieldValue.DbColumn.Key)
                {
                    keys.Add(fieldValue);
                }
            }

            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            IDbCommand ps = con.CreateCommand();
            ps.CommandText = query;
            for (int i = 0; i < keys.Count; i++)
            {
                EntityFieldValue fieldValue = keys[i];

                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.DbColumn.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.GetDataManipulate().SetToPreparedStatement(ps, fieldValue.Value, i + 1, fieldValue.DbColumn);
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterDelete(type);
            }
            ps.ExecuteNonQuery();
            DbMgmtUtility.Close(ps);
        }

        private static void SetRelationObjectKeyValues(ITypeFieldValueList valueTypeList, Type type, IEnumerable<IServerDbClass> childObjects
                , IDbRelation relation)
        {
            ICollection<IDbColumn> columns = CacheManager.FieldCache.GetDbColumns(type);
            foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
            {
                IDbColumn matchColumn = ErDataManagerUtils.FindColumnByAttribute(columns, mapping.FromField);
                EntityFieldValue fieldValue = valueTypeList.GetFieldValue(matchColumn.AttributeName);

                if (fieldValue != null)
                {
                    SetChildPrimaryKeys(fieldValue, childObjects, mapping);
                }
                else
                {
                    String message = String.Format("The column {0} does not have a matching column in the object {1}", matchColumn.ColumnName, valueTypeList.Type.FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
        }

        private static void SetChildPrimaryKeys(EntityFieldValue parentFieldValue, IEnumerable<IServerDbClass> childObjects
                , DbRelationColumnMapping mapping)
        {
            IServerRoDbClass firstObject = null;
            IEnumerator<IServerDbClass> childEnumerator = childObjects.GetEnumerator();
            if (childEnumerator.MoveNext())
            {
                firstObject = childEnumerator.Current;
            }
            if (firstObject == null)
            {
                return;
            }
            ErDataManagerUtils.RegisterTypes(firstObject);

            bool foundOnce = false;
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(firstObject.GetType(), new[] { typeof(IServerRoDbClass) });
            foreach (Type type in typeList)
            {
                ICollection<IDbColumn> subLevelColumns = CacheManager.FieldCache.GetDbColumns(type);
                IDbColumn subLevelMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (subLevelMatchedColumn != null)
                {
                    foundOnce = true;
                    PropertyInfo setter = CacheManager.MethodCache.GetProperty(firstObject, subLevelMatchedColumn.AttributeName);
                    foreach (IServerRoDbClass dbObject in childObjects)
                    {
                        setter.SetValue(dbObject, parentFieldValue.Value, null);
                    }
                }
            }
            if (!foundOnce)
            {
                String message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }
        }

        private static void SetParentRelationFieldsForNonIdentifyingRelations(IServerDbClass parentEntity, IEnumerable<IServerDbClass> childObjects
                , DbRelationColumnMapping mapping)
        {
            IServerRoDbClass firstObject = null;
            IEnumerator<IServerDbClass> childEnumerator = childObjects.GetEnumerator();
            if (childEnumerator.MoveNext())
            {
                firstObject = childEnumerator.Current;
            }
            if (firstObject == null)
            {
                return;
            }
            ErDataManagerUtils.RegisterTypes(firstObject);

            Type[] parentTypeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(parentEntity.GetType(), new[] { typeof(IServerRoDbClass) });
            Type[] childTypeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(firstObject.GetType(), new[] { typeof(IServerRoDbClass) });

            PropertyInfo setter = null;
            bool foundOnce = false;
            foreach (Type type in parentTypeList)
            {
                ICollection<IDbColumn> parentColumns = CacheManager.FieldCache.GetDbColumns(type);
                IDbColumn parentMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(parentColumns, mapping.FromField);
                if (parentMatchedColumn != null)
                {
                    foundOnce = true;
                    setter = CacheManager.MethodCache.GetProperty(parentEntity, parentMatchedColumn.AttributeName);
                }
            }
            if (!foundOnce)
            {
                String message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }

            foundOnce = false;
            foreach (Type type in childTypeList)
            {
                ICollection<IDbColumn> subLevelColumns = CacheManager.FieldCache.GetDbColumns(type);
                IDbColumn childMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (childMatchedColumn != null)
                {
                    foundOnce = true;
                    foreach (IServerRoDbClass dbObject in childObjects)
                    {
                        ITypeFieldValueList fieldValueList = ErDataManagerUtils.ExtractTypeFieldValues(dbObject, type);
                        EntityFieldValue childFieldValue = fieldValueList.GetFieldValue(childMatchedColumn.AttributeName);
                        setter.SetValue(parentEntity, childFieldValue.Value, null);
                    }
                }
            }
            if (!foundOnce)
            {
                String message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }
        }

        private static void ValidateForChildDeletion(IServerDbClass currentEntity, IEnumerable<ITypeFieldValueList> currentChildren)
        {
            foreach (ITypeFieldValueList keyValueList in currentChildren)
            {
                EntityRelationFieldValueList entityRelationKeyValueList = (EntityRelationFieldValueList)keyValueList;
                if (entityRelationKeyValueList.Relation.DeleteRule == ReferentialRuleType.Restrict
                        && !entityRelationKeyValueList.Relation.ReverseRelationship
                        && currentEntity.Status == DbClassStatus.Deleted)
                {
                    throw new IntegrityConstraintViolationException(String.Format("Cannot delete child object {0} as restrict constraint in place", keyValueList.Type.FullName));
                }
            }
        }

        private bool CheckForModification(IServerDbClass serverDBClass,IDbConnection con, IEntityContext entityContext)
        {
            if (!entityContext.ChangeTracker.Valid)
            {
                FillChangeTrackerValues(serverDBClass, con, entityContext);
            }

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(serverDBClass.GetType(), new[] { typeof(IServerDbClass) });
            foreach (Type type in typeList)
            {
                ICollection<IDbColumn> subLevelColumns = CacheManager.FieldCache.GetDbColumns(type);
                foreach (IDbColumn subLevelColumn in subLevelColumns)
                {
                    if (subLevelColumn.Key)
                    {
                        continue;
                    }

                    PropertyInfo getter = CacheManager.MethodCache.GetProperty(serverDBClass, subLevelColumn.AttributeName);
                    Object value = getter.GetValue(serverDBClass, null);

                    EntityFieldValue fieldValue = entityContext.ChangeTracker.GetFieldValue(subLevelColumn.AttributeName);
                    bool isMatch = (fieldValue != null && fieldValue.Value == value)
                            || (fieldValue != null && fieldValue.Value != null && fieldValue.Equals(value));
                    if (!isMatch)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void FillChangeTrackerValues(IServerDbClass serverDBClass, IDbConnection con, IEntityContext entityContext)
        {
            if (serverDBClass.Status == DbClassStatus.New
                    || serverDBClass.Status == DbClassStatus.Deleted)
            {
                return;
            }

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(serverDBClass.GetType(), new Type[]{typeof(IServerDbClass)});
            foreach (Type type in typeList)
            {
                String tableName = CacheManager.TableCache.GetTableName(type);
                if (tableName == null)
                {
                    continue;
                }

                ITypeFieldValueList values = ExtractCurrentRowValues(serverDBClass,type,con);
                foreach (EntityFieldValue fieldValue in values.FieldValues)
                {
                    entityContext.ChangeTracker.Fields.Add(fieldValue);
                }

                ICollection<IDbRelation> dbRelations = CacheManager.FieldCache.GetDbRelations(type);
                foreach (IDbRelation relation in dbRelations)
                {
                    ICollection<IServerRoDbClass> children = ReadRelationChildrenFromDb(serverDBClass,type,con,relation);
                    foreach (IServerRoDbClass childEntity in children)
                    {
                        ITypeFieldValueList valueTypeList = ErDataManagerUtils.ExtractRelationKeyValues(childEntity,relation);
                        if (valueTypeList != null)
                        {
                            entityContext.ChangeTracker.ChildEntityKeys.Add(valueTypeList);
                        }
                    }
                }
            }
        }

        private void DeleteOrphanChildren(IDbConnection con, IEnumerable<ITypeFieldValueList> childrenToDelete)
        {
            foreach (ITypeFieldValueList relationKeyValueList in childrenToDelete)
            {
                String table = CacheManager.TableCache.GetTableName(relationKeyValueList.Type);
                if (table == null)
                {
                    continue;
                }
                if (relationKeyValueList is EntityRelationFieldValueList)
                {
                    EntityRelationFieldValueList entityRelationFieldValueList = (EntityRelationFieldValueList) relationKeyValueList;
                    if (entityRelationFieldValueList.Relation.ReverseRelationship 
                        || entityRelationFieldValueList.Relation.NonIdentifyingRelation)
                    {
                        continue;
                    }
                }

                bool recordExists = false;
                IDbCommand cmd = CreateRetrievalPreparedStatement(relationKeyValueList,con);
                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    recordExists = true;
                }
                DbMgmtUtility.Close(reader);
                DbMgmtUtility.Close(cmd);

                if (recordExists)
                {
                    Delete(relationKeyValueList,relationKeyValueList.Type,con);
                }
            }
        }

        private bool VersionValidated(IServerRoDbClass entity, Type type, IDbConnection con)
        {
            ICollection<IDbColumn> typeColumns = CacheManager.FieldCache.GetDbColumns(type);
            foreach (IDbColumn typeColumn in typeColumns)
            {
                if (typeColumn.ColumnType == DbColumnType.Version)
                {
                    Object classValue = ExtractCurrentVersionValue(entity,typeColumn,type,con);
                    EntityFieldValue originalFieldValue = entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName);
                    return originalFieldValue != null && classValue == originalFieldValue.Value
                            || (originalFieldValue != null && classValue != null && classValue.Equals(originalFieldValue.Value));
                }
            }

            ITypeFieldValueList fieldValueList = ExtractCurrentRowValues(entity,type,con);
            if (fieldValueList == null)
            {
                return false;
            }
            foreach (IDbColumn typeColumn in typeColumns)
            {
                EntityFieldValue classFieldValue = fieldValueList.GetFieldValue(typeColumn.AttributeName);
                EntityFieldValue originalFieldValue = entity.Context != null ? entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName) : null;
                bool matches = originalFieldValue != null && classFieldValue != null && classFieldValue.Value == originalFieldValue.Value
                            || (originalFieldValue != null && classFieldValue != null && classFieldValue.Value.Equals(originalFieldValue.Value));
                if (!matches)
                {
                    return false;
                }
            }
            return true;
        }

        private Object ExtractCurrentVersionValue(IServerRoDbClass entity, IDbColumn versionColumn, Type type
            , IDbConnection con)
        {
            Object versionValue = null;

            ITypeFieldValueList keyFieldValueList = ErDataManagerUtils.ExtractTypeKeyValues(entity, type);
            IDbCommand cmd = CreateRetrievalPreparedStatement(keyFieldValueList, con);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                versionValue = DbLayer.GetDataManipulate().ReadFromResultSet(reader, versionColumn);
            }
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);

            return versionValue;
        }

        private ITypeFieldValueList ExtractCurrentRowValues(IServerRoDbClass entity, Type type, IDbConnection con)
        {
            ITypeFieldValueList fieldValueList = null;

            ITypeFieldValueList keyFieldValueList = ErDataManagerUtils.ExtractTypeKeyValues(entity, type);
            IDbCommand cmd = CreateRetrievalPreparedStatement(keyFieldValueList, con);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                fieldValueList = ReadValues(type, reader);
            }
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);

            return fieldValueList;
        }
    }
}
