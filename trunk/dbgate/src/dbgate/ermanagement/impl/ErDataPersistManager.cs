using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Text;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.caches.impl;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.exceptions.common;
using dbgate.ermanagement.exceptions.persist;
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
                TrackAndCommitChanges(entity, con);

                var entityInfoStack = new Stack<EntityInfo>();
                EntityInfo entityInfo = CacheManager.GetEntityInfo(entity);
                while (entityInfo != null)
                {
                    entityInfoStack.Push(entityInfo);
                    entityInfo = entityInfo.SuperEntityInfo;
                }
              
                while (entityInfoStack.Count != 0)
                {
                    entityInfo = entityInfoStack.Pop();
                    SaveForType(entity, entityInfo.EntityType, con);
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
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            if (entityInfo == null)
            {
                return;
            }

            ICollection<IDbRelation> dbRelations = entityInfo.Relations;
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
                            SetParentRelationFieldsForNonIdentifyingRelations(entity,relation.RelatedObjectType, childObjects, mapping);
                        }
                    }
                }
            }

            ITypeFieldValueList fieldValues = ErDataManagerUtils.ExtractEntityTypeFieldValues(entity, type);
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
                Update(entity, fieldValues, type, con);
            }
            else if (entity.Status == DbClassStatus.Deleted)
            {
                Delete(fieldValues, type, con);
            }
            else
            {
                string message = String.Format("In-corret status for class {0}", type.FullName);
                throw new IncorrectStatusException(message);
            }

            fieldValues = ErDataManagerUtils.ExtractEntityTypeFieldValues(entity, type);
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

                    SetRelationObjectKeyValues(fieldValues, type,relation.RelatedObjectType, childObjects, relation);
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

        private void Insert(IServerDbClass entity,ITypeFieldValueList valueTypeList, Type entityType, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);

            StringBuilder logSb = new StringBuilder();
            string query = entityInfo.GetInsertQuery(DbLayer);
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
                    PropertyInfo setter = entityType.GetProperty(dbColumn.AttributeName);
                    ReflectionUtils.SetValue(setter,entity,columnValue);
                }
                else
                {
                    columnValue = fieldValue.Value;
                }
                if (showQuery)
                {
                    logSb.Append(" ,").Append(dbColumn.ColumnName).Append("=").Append(columnValue);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, columnValue, i + 1, dbColumn);
                i++;
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterInsert(entityType);
            }
            cmd.ExecuteNonQuery();
            DbMgmtUtility.Close(cmd);
        }

        private void Update(IServerDbClass entity, ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<EntityFieldValue> keys = new List<EntityFieldValue>();
            ICollection<EntityFieldValue> values = new List<EntityFieldValue>();
            StringBuilder logSb = new StringBuilder();
            string query;
            

            if (Config.UpdateChangedColumnsOnly)
            {
                values = GetModifiedFieldValues(entity, type);
                if (values.Count == 0)
                    return;

                keys = ErDataManagerUtils.ExtractEntityKeyValues(entity).FieldValues;
                ICollection<IDbColumn> keysAndModified = new List<IDbColumn>();
                foreach (EntityFieldValue fieldValue in values)
                {
                    keysAndModified.Add(fieldValue.DbColumn);
                }
                foreach (EntityFieldValue fieldValue in keys)
                {
                    keysAndModified.Add(fieldValue.DbColumn);
                }
                query = DbLayer.DataManipulate().CreateUpdateQuery(entityInfo.TableName, keysAndModified);
            }
            else
            {
                query = entityInfo.GetUpdateQuery(DbLayer);
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
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.DbColumn);
            }

            foreach (EntityFieldValue fieldValue in keys)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.DbColumn.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.DbColumn);
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
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            StringBuilder logSb = new StringBuilder();
            string query = entityInfo.GetDeleteQuery(DbLayer);
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
                DbLayer.DataManipulate().SetToPreparedStatement(ps, fieldValue.Value, i + 1, fieldValue.DbColumn);
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

        private static void SetRelationObjectKeyValues(ITypeFieldValueList valueTypeList, Type entityType,Type childEntityType
            , IEnumerable<IServerDbClass> childObjects, IDbRelation relation)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);
            ICollection<IDbColumn> columns = entityInfo.Columns;
            foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
            {
                IDbColumn matchColumn = ErDataManagerUtils.FindColumnByAttribute(columns, mapping.FromField);
                EntityFieldValue fieldValue = valueTypeList.GetFieldValue(matchColumn.AttributeName);

                if (fieldValue != null)
                {
                    SetChildPrimaryKeys(fieldValue,childEntityType, childObjects, mapping);
                }
                else
                {
                    string message = String.Format("The column {0} does not have a matching column in the object {1}", matchColumn.ColumnName, valueTypeList.Type.FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
        }

        private static void SetChildPrimaryKeys(EntityFieldValue parentFieldValue,Type childEntityType
            , IEnumerable<IServerDbClass> childObjects, DbRelationColumnMapping mapping)
        {
            bool foundOnce = false;
            EntityInfo parentEntityInfo = CacheManager.GetEntityInfo(childEntityType);
            EntityInfo entityInfo = parentEntityInfo;

            while (entityInfo != null)
            {
                ICollection<IDbColumn> subLevelColumns = entityInfo.Columns;
                IDbColumn subLevelMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (subLevelMatchedColumn != null)
                {
                    foundOnce = true;
                    PropertyInfo setter = parentEntityInfo.GetProperty(subLevelMatchedColumn);
                    foreach (IServerRoDbClass dbObject in childObjects)
                    {
                        ReflectionUtils.SetValue(setter, dbObject, parentFieldValue.Value);
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                string message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, childEntityType.FullName);
                throw new NoMatchingColumnFoundException(message);
            }
        }

        private static void SetParentRelationFieldsForNonIdentifyingRelations(IServerDbClass parentEntity,Type childEntityType
            , IEnumerable<IServerDbClass> childObjects, DbRelationColumnMapping mapping)
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
            EntityInfo parentInfo = CacheManager.GetEntityInfo(parentEntity);
            EntityInfo childInfo = CacheManager.GetEntityInfo(firstObject);

            PropertyInfo setter = null;
            bool foundOnce = false;
            while (parentInfo != null)
            {
                ICollection<IDbColumn> parentColumns = parentInfo.Columns;
                IDbColumn parentMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(parentColumns, mapping.FromField);
                if (parentMatchedColumn != null)
                {
                    foundOnce = true;
                    setter = parentInfo.GetProperty(parentMatchedColumn);
                }
                parentInfo = parentInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                string message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }

            foundOnce = false;
            while (childInfo != null)
            {
                ICollection<IDbColumn> subLevelColumns = childInfo.Columns;
                IDbColumn childMatchedColumn = ErDataManagerUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (childMatchedColumn != null)
                {
                    foundOnce = true;
                    foreach (IServerRoDbClass dbObject in childObjects)
                    {
                        ITypeFieldValueList fieldValueList = ErDataManagerUtils.ExtractEntityTypeFieldValues(dbObject, childInfo.EntityType);
                        EntityFieldValue childFieldValue = fieldValueList.GetFieldValue(childMatchedColumn.AttributeName);
                        setter.SetValue(parentEntity, childFieldValue.Value, null);
                    }
                }
                childInfo = childInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                string message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
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

        private bool CheckForModification(IServerDbClass entity,IDbConnection con, IEntityContext entityContext)
        {
            if (!entityContext.ChangeTracker.Valid)
            {
                FillChangeTrackerValues(entity, con, entityContext);
            }

            EntityInfo entityInfo = CacheManager.GetEntityInfo(entity);
            while (entityInfo != null)
            {
                ICollection<IDbColumn> subLevelColumns = entityInfo.Columns;
                foreach (IDbColumn subLevelColumn in subLevelColumns)
                {
                    if (subLevelColumn.Key)
                    {
                        continue;
                    }

                    PropertyInfo getter = entityInfo.GetProperty(subLevelColumn.AttributeName);
                    Object value = ReflectionUtils.GetValue(getter,entity);

                    EntityFieldValue fieldValue = entityContext.ChangeTracker.GetFieldValue(subLevelColumn.AttributeName);
                    bool isMatch = (fieldValue != null && fieldValue.Value == value)
                            || (fieldValue != null && fieldValue.Value != null && fieldValue.Equals(value));
                    if (!isMatch)
                    {
                        return true;
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
            return false;
        }

        private void FillChangeTrackerValues(IServerDbClass entity, IDbConnection con, IEntityContext entityContext)
        {
            if (entity.Status == DbClassStatus.New
                    || entity.Status == DbClassStatus.Deleted)
            {
                return;
            }

            EntityInfo entityInfo = CacheManager.GetEntityInfo(entity);
            while (entityInfo != null)
            {
                ITypeFieldValueList values = ExtractCurrentRowValues(entity,entityInfo.EntityType,con);
                foreach (EntityFieldValue fieldValue in values.FieldValues)
                {
                    entityContext.ChangeTracker.Fields.Add(fieldValue);
                }

                ICollection<IDbRelation> dbRelations = entityInfo.Relations;
                foreach (IDbRelation relation in dbRelations)
                {
                    ICollection<IServerRoDbClass> children = ReadRelationChildrenFromDb(entity,entityInfo.EntityType,con,relation);
                    foreach (IServerRoDbClass childEntity in children)
                    {
                        ITypeFieldValueList valueTypeList = ErDataManagerUtils.ExtractRelationKeyValues(childEntity,relation);
                        if (valueTypeList != null)
                        {
                            entityContext.ChangeTracker.ChildEntityKeys.Add(valueTypeList);
                        }
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
        }

        private void DeleteOrphanChildren(IDbConnection con, IEnumerable<ITypeFieldValueList> childrenToDelete)
        {
            foreach (ITypeFieldValueList relationKeyValueList in childrenToDelete)
            {
                EntityInfo entityInfo = CacheManager.GetEntityInfo(relationKeyValueList.Type);
                if (entityInfo == null)
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
                IDbCommand cmd = null;
                IDataReader reader = null;
                try
                {
                    cmd = CreateRetrievalPreparedStatement(relationKeyValueList, con);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        recordExists = true;
                    }
                }
                catch (Exception ex)
                {
                    string message =
                        String.Format(
                            "SQL Exception while trying determine if orphan child entities are available for type {0}",
                            relationKeyValueList.Type.FullName);
                     throw new StatementExecutionException(message, ex);  
                }
                finally
                {
                    DbMgmtUtility.Close(reader);
                    DbMgmtUtility.Close(cmd);
                }
                
                if (recordExists)
                {
                    Delete(relationKeyValueList,relationKeyValueList.Type,con);
                }
            }
        }

        private bool VersionValidated(IServerRoDbClass entity, Type type, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<IDbColumn> typeColumns = entityInfo.Columns;
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

            if (Config.UpdateChangedColumnsOnly)
		 	{
		 	    ICollection<EntityFieldValue> modified = GetModifiedFieldValues(entity, type);
		 	    typeColumns = new List<IDbColumn>();
		 	    foreach (EntityFieldValue fieldValue in modified)
		 	    {
		 	        typeColumns.Add(fieldValue.DbColumn);
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
                            || (originalFieldValue != null && classFieldValue != null && classFieldValue.Value != null && classFieldValue.Value.Equals(originalFieldValue.Value));
                if (!matches)
                {
                    return false;
                }
            }
            return true;
        }

        private ICollection<EntityFieldValue> GetModifiedFieldValues(IServerRoDbClass entity, Type type)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<IDbColumn> typeColumns = entityInfo.Columns;
            ITypeFieldValueList currentValues = ErDataManagerUtils.ExtractEntityTypeFieldValues(entity,type);
            ICollection<EntityFieldValue> modifiedColumns = new  List<EntityFieldValue>();

            foreach (IDbColumn typeColumn in typeColumns)
            {
                if (typeColumn.Key)
                    continue;

                EntityFieldValue classFieldValue = currentValues.GetFieldValue(typeColumn.AttributeName);
                EntityFieldValue originalFieldValue = entity.Context != null ? entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName) : null;
                bool matches = originalFieldValue != null && classFieldValue != null && classFieldValue.Value == originalFieldValue.Value
                        || (originalFieldValue != null && classFieldValue != null && classFieldValue.Value != null && classFieldValue.Value.Equals(originalFieldValue.Value));
                if (!matches)
                {
                    modifiedColumns.Add(classFieldValue);
                }
            }
            return modifiedColumns;
        }

        private Object ExtractCurrentVersionValue(IServerRoDbClass entity, IDbColumn versionColumn, Type type
            , IDbConnection con)
        {
            Object versionValue = null;

            ITypeFieldValueList keyFieldValueList = ErDataManagerUtils.ExtractEntityTypeKeyValues(entity, type);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = CreateRetrievalPreparedStatement(keyFieldValueList, con);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    versionValue = DbLayer.DataManipulate().ReadFromResultSet(reader, versionColumn);
                }
            }
            catch (Exception ex)
            {
                String message = String.Format("SQL Exception while trying retrieve version information from {0}"
                                               , entity.GetType().FullName);
                throw new StatementExecutionException(message, ex);
            }
            finally
            {
                DbMgmtUtility.Close(reader);
                DbMgmtUtility.Close(cmd);  
            }
            return versionValue;
        }

        private ITypeFieldValueList ExtractCurrentRowValues(IServerRoDbClass entity, Type type, IDbConnection con)
        {
            ITypeFieldValueList fieldValueList = null;

            ITypeFieldValueList keyFieldValueList = ErDataManagerUtils.ExtractEntityTypeKeyValues(entity, type);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = CreateRetrievalPreparedStatement(keyFieldValueList, con);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    fieldValueList = ReadValues(type, reader);
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("SQL Exception while trying retrieve current data row from {0}"
                                               , entity.GetType().FullName);
                throw new StatementExecutionException(message, ex);
            }
            finally
            {
                DbMgmtUtility.Close(reader);
                DbMgmtUtility.Close(cmd);   
            }
            return fieldValueList;
        }
    }
}
