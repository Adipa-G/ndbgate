using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exceptions;
using DbGate.Exceptions.Common;
using DbGate.Exceptions.Persist;
using DbGate.Utility;
using log4net;

namespace DbGate.ErManagement.ErMapper
{
    public class PersistOperationLayer : BaseOperationLayer
    {
        public PersistOperationLayer(IDbLayer dbLayer,IDbGateStatistics statistics, IDbGateConfig config)
            : base(dbLayer,statistics, config)
        {
        }

        public void Save(IEntity entity, ITransaction tx)
        {
            try
            {
                TrackAndCommitChanges(entity, tx);

                var entityInfoStack = new Stack<EntityInfo>();
                var entityInfo = CacheManager.GetEntityInfo(entity);
                while (entityInfo != null)
                {
                    entityInfoStack.Push(entityInfo);
                    entityInfo = entityInfo.SuperEntityInfo;
                }
              
                while (entityInfoStack.Count != 0)
                {
                    entityInfo = entityInfoStack.Pop();
                    SaveForType(entity, entityInfo.EntityType, tx);
                }
                entity.Status = EntityStatus.Unmodified;
                entity.Context.DestroyReferenceStore();
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(e.Message, e);
                throw new PersistException(e.Message, e);
            }
        }

        private void TrackAndCommitChanges(IEntity entity, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(entity);
            var entityContext = entity.Context;
            IEnumerable<ITypeFieldValueList> originalChildren;

            if (entityInfo.TableInfo.DirtyCheckStrategy == DirtyCheckStrategy.Automatic)
            {
                if (CheckForModification(entity, tx, entityContext))
                {
                    MiscUtils.Modify(entity);
                }
            }

            if (entityContext.ChangeTracker.Valid)
            {
                originalChildren = entityContext.ChangeTracker.ChildEntityKeys;
            }
            else
            {
                originalChildren = GetChildEntityValueListIncludingDeletedStatusItems(entity);
            }

            var currentChildren = GetChildEntityValueListExcludingDeletedStatusItems(entity);
            ValidateForChildDeletion(entity, currentChildren);
            var deletedChildren = OperationUtils.FindDeletedChildren(originalChildren, currentChildren);
            DeleteOrphanChildren(tx, deletedChildren);
        }

        private void SaveForType(IEntity entity, Type type, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            if (entityInfo == null)
            {
                return;
            }

            ProcessNonIdentifyingRelations(entity, entityInfo);

            var fieldValues = OperationUtils.ExtractEntityTypeFieldValues(entity, type);
            if (entity.Status == EntityStatus.Unmodified)
            {
                //do nothing
            }
            else if (entity.Status == EntityStatus.New)
            {
                Insert(entity,fieldValues, type, tx);
            }
            else if (entity.Status == EntityStatus.Modified)
            {
                if (entityInfo.TableInfo.VerifyOnWriteStrategy == VerifyOnWriteStrategy.Verify)
                {
                    if (!VersionValidated(entity, type, tx))
                    {
                        throw new DataUpdatedFromAnotherSourceException(String.Format("The type {0} updated from another transaction", type));
                    }
                    OperationUtils.IncrementVersion(fieldValues);
                    SetValues(entity, fieldValues);
                }
                Update(entity, fieldValues, type, tx);
            }
            else if (entity.Status == EntityStatus.Deleted)
            {
                Delete(fieldValues, type, tx);
            }
            else
            {
                var message = String.Format("In-corret status for class {0}", type.FullName);
                throw new IncorrectStatusException(message);
            }

            fieldValues = OperationUtils.ExtractEntityTypeFieldValues(entity, type);
            var entityContext = entity.Context;
            if (entityContext != null)
            {
                entityContext.ChangeTracker.AddFields(fieldValues.FieldValues);
            }
            entity.Context.AddToCurrentObjectGraphIndex(entity);

            ProcessIdentifyingRelations(entity, type, tx, entityInfo, fieldValues);
        }

        private static void ProcessIdentifyingRelations(IEntity entity, Type type, ITransaction tx
            , EntityInfo entityInfo,ITypeFieldValueList fieldValues)
        {
            var dbRelations = entityInfo.Relations;
            foreach (var relation in dbRelations)
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

                var childObjects = OperationUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {
                    SetRelationObjectKeyValues(fieldValues, type, relation.RelatedObjectType, childObjects, relation);
                    foreach (var fieldObject in childObjects)
                    {
                        var childEntityKeyList = OperationUtils.ExtractEntityKeyValues(fieldObject);
                        if (entity.Context.AlreadyInCurrentObjectGraph(childEntityKeyList))
                        {
                            continue;
                        }
                        fieldObject.Context.CopyReferenceStoreFrom(entity);
                        if (fieldObject.Status != EntityStatus.Deleted) //deleted items are already deleted
                        {
                            fieldObject.Persist(tx);
                            entity.Context.AddToCurrentObjectGraphIndex(fieldObject);
                        }
                    }
                }
            }
        }

        private static void ProcessNonIdentifyingRelations(IEntity entity, EntityInfo entityInfo)
        {
            var dbRelations = entityInfo.Relations;
            foreach (var relation in dbRelations)
            {
                if (relation.ReverseRelationship)
                {
                    continue;
                }
                if (IsProxyObject(entity, relation))
                {
                    continue;
                }
                var childObjects = OperationUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {
                    if (relation.NonIdentifyingRelation)
                    {
                        foreach (var mapping in relation.TableColumnMappings)
                        {
                            if (entityInfo.FindRelationColumnInfo(mapping.FromField) == null)
                            {
                                SetParentRelationFieldsForNonIdentifyingRelations(entity, childObjects, mapping);
                            }
                        }
                    }
                }
            }
        }

        private void Insert(IEntity entity,ITypeFieldValueList valueTypeList, Type entityType, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(entityType);

            var logSb = new StringBuilder();
            var query = entityInfo.GetInsertQuery(DbLayer);
            var cmd = tx.CreateCommand();
            cmd.CommandText = query;

            var showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            var i = 0;
            foreach (var fieldValue in valueTypeList.FieldValues)
            {
                var column = fieldValue.Column;
                Object columnValue;

                if (column.ReadFromSequence
                        && column.SequenceGenerator != null)
                {
                    columnValue = column.SequenceGenerator.GetNextSequenceValue(tx);
                    var setter = entityType.GetProperty(column.AttributeName);
                    ReflectionUtils.SetValue(setter,entity,columnValue);
                }
                else
                {
                    columnValue = fieldValue.Value;
                }
                if (showQuery)
                {
                    logSb.Append(" ,").Append(column.ColumnName).Append("=").Append(columnValue);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, columnValue, i + 1, column);
                i++;
            }
            if (showQuery)
            {
                Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterInsert(entityType);
            }
            cmd.ExecuteNonQuery();
            DbMgtUtility.Close(cmd);
        }

        private void Update(IEntity entity, ITypeFieldValueList valueTypeList, Type type, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<EntityFieldValue> keys = new List<EntityFieldValue>();
            ICollection<EntityFieldValue> values = new List<EntityFieldValue>();
            var logSb = new StringBuilder();
            string query;
            

            if (entityInfo.TableInfo.UpdateStrategy == UpdateStrategy.ChangedColumns)
            {
                values = GetModifiedFieldValues(entity, type);
                if (values.Count == 0)
                    return;

                keys = OperationUtils.ExtractEntityKeyValues(entity).FieldValues;
                ICollection<IColumn> keysAndModified = new List<IColumn>();
                foreach (var fieldValue in values)
                {
                    keysAndModified.Add(fieldValue.Column);
                }
                foreach (var fieldValue in keys)
                {
                    keysAndModified.Add(fieldValue.Column);
                }
                query = DbLayer.DataManipulate().CreateUpdateQuery(entityInfo.TableInfo.TableName, keysAndModified);
            }
            else
            {
                query = entityInfo.GetUpdateQuery(DbLayer);
                foreach (var fieldValue in valueTypeList.FieldValues)
                {
                    if (fieldValue.Column.Key)
                    {
                        keys.Add(fieldValue);
                    }
                    else
                    {
                        values.Add(fieldValue);
                    }
                }
            }

            var showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }

            var cmd = tx.CreateCommand();
            cmd.CommandText = query;
            var count = 0;
            foreach (var fieldValue in values)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.Column);
            }

            foreach (var fieldValue in keys)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.Column);
            }
            if (showQuery)
            {
                Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterUpdate(type);
            }
            cmd.ExecuteNonQuery();
            DbMgtUtility.Close(cmd);
        }

        private void Delete(ITypeFieldValueList valueTypeList, Type type, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            var logSb = new StringBuilder();
            var query = entityInfo.GetDeleteQuery(DbLayer);
            IList<EntityFieldValue> keys = new List<EntityFieldValue>();

            foreach (var fieldValue in valueTypeList.FieldValues)
            {
                if (fieldValue.Column.Key)
                {
                    keys.Add(fieldValue);
                }
            }

            var showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            var ps = tx.CreateCommand();
            ps.CommandText = query;
            for (var i = 0; i < keys.Count; i++)
            {
                var fieldValue = keys[i];

                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(ps, fieldValue.Value, i + 1, fieldValue.Column);
            }
            if (showQuery)
            {
                Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterDelete(type);
            }
            ps.ExecuteNonQuery();
            DbMgtUtility.Close(ps);
        }

        private static void SetRelationObjectKeyValues(ITypeFieldValueList valueTypeList, Type entityType,Type childEntityType
            , IEnumerable<IEntity> childObjects, IRelation relation)
        {
            var entityInfo = CacheManager.GetEntityInfo(entityType);
            var columns = entityInfo.Columns;
            foreach (var mapping in relation.TableColumnMappings)
            {
                var matchColumn = entityInfo.FindColumnByAttribute(mapping.FromField);
                var fieldValue = valueTypeList.GetFieldValue(matchColumn.AttributeName);

                if (fieldValue != null)
                {
                    SetChildPrimaryKeys(fieldValue,childEntityType, childObjects, mapping);
                }
                else
                {
                    var message = String.Format("The column {0} does not have a matching column in the object {1}", matchColumn.ColumnName, valueTypeList.Type.FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
        }

        private static void SetChildPrimaryKeys(EntityFieldValue parentFieldValue,Type childEntityType
            , IEnumerable<IEntity> childObjects, RelationColumnMapping mapping)
        {
            var foundOnce = false;
            var parentEntityInfo = CacheManager.GetEntityInfo(childEntityType);
            var entityInfo = parentEntityInfo;

            while (entityInfo != null)
            {
                var subLevelMatchedColumn = entityInfo.FindColumnByAttribute(mapping.ToField);

                if (subLevelMatchedColumn != null)
                {
                    foundOnce = true;
                    var setter = parentEntityInfo.GetProperty(subLevelMatchedColumn);
                    foreach (IReadOnlyEntity dbObject in childObjects)
                    {
                        ReflectionUtils.SetValue(setter, dbObject, parentFieldValue.Value);
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                var message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, childEntityType.FullName);
                throw new NoMatchingColumnFoundException(message);
            }
        }

        private static void SetParentRelationFieldsForNonIdentifyingRelations(IEntity parentEntity
            ,IEnumerable<IEntity> childObjects, RelationColumnMapping mapping)
        {
            IReadOnlyEntity firstObject = null;
            var childEnumerator = childObjects.GetEnumerator();
            if (childEnumerator.MoveNext())
            {
                firstObject = childEnumerator.Current;
            }
            if (firstObject == null)
            {
                return;
            }
            var parentInfo = CacheManager.GetEntityInfo(parentEntity);
            var childInfo = CacheManager.GetEntityInfo(firstObject);

            PropertyInfo setter = null;
            var foundOnce = false;
            while (parentInfo != null)
            {
                var parentMatchedColumn = parentInfo.FindColumnByAttribute(mapping.FromField);
                if (parentMatchedColumn != null)
                {
                    foundOnce = true;
                    setter = parentInfo.GetProperty(parentMatchedColumn);
                }
                parentInfo = parentInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                var message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }

            foundOnce = false;
            while (childInfo != null)
            {
                var childMatchedColumn = childInfo.FindColumnByAttribute(mapping.ToField);

                if (childMatchedColumn != null)
                {
                    foundOnce = true;
                    foreach (IReadOnlyEntity dbObject in childObjects)
                    {
                        var fieldValueList = OperationUtils.ExtractEntityTypeFieldValues(dbObject, childInfo.EntityType);
                        var childFieldValue = fieldValueList.GetFieldValue(childMatchedColumn.AttributeName);
                        setter.SetValue(parentEntity, childFieldValue.Value, null);
                    }
                }
                childInfo = childInfo.SuperEntityInfo;
            }
            if (!foundOnce)
            {
                var message = String.Format("The field {0} does not have a matching field in the object {1}", mapping.ToField, firstObject.GetType().FullName);
                throw new NoMatchingColumnFoundException(message);
            }
        }

        private static void ValidateForChildDeletion(IEntity currentEntity, IEnumerable<ITypeFieldValueList> currentChildren)
        {
            foreach (var keyValueList in currentChildren)
            {
                var entityRelationKeyValueList = (EntityRelationFieldValueList)keyValueList;
                if (entityRelationKeyValueList.Relation.DeleteRule == ReferentialRuleType.Restrict
                        && !entityRelationKeyValueList.Relation.ReverseRelationship
                        && currentEntity.Status == EntityStatus.Deleted)
                {
                    throw new IntegrityConstraintViolationException(String.Format("Cannot delete child object {0} as restrict constraint in place", keyValueList.Type.FullName));
                }
            }
        }

        private bool CheckForModification(IEntity entity,ITransaction tx, IEntityContext entityContext)
        {
            if (!entityContext.ChangeTracker.Valid)
            {
                FillChangeTrackerValues(entity, tx, entityContext);
            }

            var entityInfo = CacheManager.GetEntityInfo(entity);
            while (entityInfo != null)
            {
                var subLevelColumns = entityInfo.Columns;
                foreach (var subLevelColumn in subLevelColumns)
                {
                    if (subLevelColumn.Key)
                    {
                        continue;
                    }

                    var getter = entityInfo.GetProperty(subLevelColumn.AttributeName);
                    var value = ReflectionUtils.GetValue(getter,entity);

                    var fieldValue = entityContext.ChangeTracker.GetFieldValue(subLevelColumn.AttributeName);
                    var isMatch = (fieldValue != null && fieldValue.Value == value)
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

        private void FillChangeTrackerValues(IEntity entity, ITransaction tx, IEntityContext entityContext)
        {
            if (entity.Status == EntityStatus.New
                    || entity.Status == EntityStatus.Deleted)
            {
                return;
            }

            var entityInfo = CacheManager.GetEntityInfo(entity);
            while (entityInfo != null)
            {
                var values = ExtractCurrentRowValues(entity,entityInfo.EntityType,tx);
                entityContext.ChangeTracker.AddFields(values.FieldValues);
                
                var dbRelations = entityInfo.Relations;
                foreach (var relation in dbRelations)
                {
                    var children = ReadRelationChildrenFromDb(entity,entityInfo.EntityType,tx,relation);
                    foreach (var childEntity in children)
                    {
                        var valueTypeList = OperationUtils.ExtractRelationKeyValues(childEntity,relation);
                        if (valueTypeList != null)
                        {
                            entityContext.ChangeTracker.AddChildEntityKey(valueTypeList);
                        }
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
        }

        private void DeleteOrphanChildren(ITransaction tx, IEnumerable<ITypeFieldValueList> childrenToDelete)
        {
            foreach (var relationKeyValueList in childrenToDelete)
            {
                var entityInfo = CacheManager.GetEntityInfo(relationKeyValueList.Type);
                if (entityInfo == null)
                {
                    continue;
                }

                if (relationKeyValueList is EntityRelationFieldValueList)
                {
                    var entityRelationFieldValueList = (EntityRelationFieldValueList) relationKeyValueList;
                    if (entityRelationFieldValueList.Relation.ReverseRelationship 
                        || entityRelationFieldValueList.Relation.NonIdentifyingRelation)
                    {
                        continue;
                    }
                }

                var recordExists = false;
                IDbCommand cmd = null;
                IDataReader reader = null;
                try
                {
                    cmd = CreateRetrievalPreparedStatement(relationKeyValueList, tx);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        recordExists = true;
                    }
                }
                catch (Exception ex)
                {
                    var message =
                        String.Format(
                            "SQL Exception while trying determine if orphan child entities are available for type {0}",
                            relationKeyValueList.Type.FullName);
                     throw new StatementExecutionException(message, ex);  
                }
                finally
                {
                    DbMgtUtility.Close(reader);
                    DbMgtUtility.Close(cmd);
                }
                
                if (recordExists)
                {
                    Delete(relationKeyValueList,relationKeyValueList.Type,tx);
                }
            }
        }

        private bool VersionValidated(IReadOnlyEntity entity, Type type, ITransaction tx)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            var typeColumns = entityInfo.Columns;
            foreach (var typeColumn in typeColumns)
            {
                if (typeColumn.ColumnType == ColumnType.Version)
                {
                    var classValue = ExtractCurrentVersionValue(entity,typeColumn,type,tx);
                    var originalFieldValue = entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName);
                    return originalFieldValue != null && classValue == originalFieldValue.Value
                            || (originalFieldValue != null && classValue != null && classValue.Equals(originalFieldValue.Value));
                }
            }

            if (entityInfo.TableInfo.UpdateStrategy == UpdateStrategy.ChangedColumns)
		 	{
		 	    var modified = GetModifiedFieldValues(entity, type);
		 	    typeColumns = new List<IColumn>();
		 	    foreach (var fieldValue in modified)
		 	    {
		 	        typeColumns.Add(fieldValue.Column);
		 	    }
		 	}

            var fieldValueList = ExtractCurrentRowValues(entity,type,tx);
            if (fieldValueList == null)
            {
                return false;
            }
            foreach (var typeColumn in typeColumns)
            {
                var classFieldValue = fieldValueList.GetFieldValue(typeColumn.AttributeName);
                var originalFieldValue = entity.Context != null ? entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName) : null;
                var matches = originalFieldValue != null && classFieldValue != null && classFieldValue.Value == originalFieldValue.Value
                              || (originalFieldValue != null && classFieldValue != null && classFieldValue.Value != null && classFieldValue.Value.Equals(originalFieldValue.Value));
                if (!matches)
                {
                    return false;
                }
            }
            return true;
        }

        private ICollection<EntityFieldValue> GetModifiedFieldValues(IReadOnlyEntity entity, Type type)
        {
            var entityInfo = CacheManager.GetEntityInfo(type);
            var typeColumns = entityInfo.Columns;
            var currentValues = OperationUtils.ExtractEntityTypeFieldValues(entity,type);
            ICollection<EntityFieldValue> modifiedColumns = new  List<EntityFieldValue>();

            foreach (var typeColumn in typeColumns)
            {
                if (typeColumn.Key)
                    continue;

                var classFieldValue = currentValues.GetFieldValue(typeColumn.AttributeName);
                var originalFieldValue = entity.Context != null ? entity.Context.ChangeTracker.GetFieldValue(typeColumn.AttributeName) : null;
                var matches = originalFieldValue != null && classFieldValue != null && classFieldValue.Value == originalFieldValue.Value
                              || (originalFieldValue != null && classFieldValue != null && classFieldValue.Value != null && classFieldValue.Value.Equals(originalFieldValue.Value));
                if (!matches)
                {
                    modifiedColumns.Add(classFieldValue);
                }
            }
            return modifiedColumns;
        }

        private Object ExtractCurrentVersionValue(IReadOnlyEntity entity, IColumn versionColumn, Type type
            , ITransaction tx)
        {
            Object versionValue = null;

            var keyFieldValueList = OperationUtils.ExtractEntityTypeKeyValues(entity, type);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = CreateRetrievalPreparedStatement(keyFieldValueList, tx);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    versionValue = DbLayer.DataManipulate().ReadFromResultSet(reader, versionColumn);
                }
            }
            catch (Exception ex)
            {
                var message = String.Format("SQL Exception while trying retrieve version information from {0}"
                                               , entity.GetType().FullName);
                throw new StatementExecutionException(message, ex);
            }
            finally
            {
                DbMgtUtility.Close(reader);
                DbMgtUtility.Close(cmd);  
            }
            return versionValue;
        }

        private ITypeFieldValueList ExtractCurrentRowValues(IReadOnlyEntity entity, Type type, ITransaction tx)
        {
            ITypeFieldValueList fieldValueList = null;

            var keyFieldValueList = OperationUtils.ExtractEntityTypeKeyValues(entity, type);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = CreateRetrievalPreparedStatement(keyFieldValueList, tx);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    fieldValueList = ReadValues(type, reader);
                }
            }
            catch (Exception ex)
            {
                var message = String.Format("SQL Exception while trying retrieve current data row from {0}"
                                               , entity.GetType().FullName);
                throw new StatementExecutionException(message, ex);
            }
            finally
            {
                DbMgtUtility.Close(reader);
                DbMgtUtility.Close(cmd);   
            }
            return fieldValueList;
        }
    }
}
