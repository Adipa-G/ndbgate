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

        public void Save(IEntity entity, IDbConnection con)
        {
            try
            {
                SessionUtils.InitSession(entity);
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
                entity.Status = EntityStatus.Unmodified;
                SessionUtils.DestroySession(entity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(e.Message, e);
                throw new PersistException(e.Message, e);
            }
        }

        private void TrackAndCommitChanges(IEntity entity, IDbConnection con)
        {
            IEntityContext entityContext = entity.Context;
            ICollection<ITypeFieldValueList> originalChildren;

            if (entityContext != null)
            {
                if (Config.AutoTrackChanges)
                {
                    if (CheckForModification(entity,con, entityContext))
                    {
                        MiscUtils.Modify(entity);
                    }
                }
                originalChildren = entityContext.ChangeTracker.ChildEntityKeys;
            }
            else
            {
                originalChildren = GetChildEntityValueListIncludingDeletedStatusItems(entity);
            }

            ICollection<ITypeFieldValueList> currentChildren = GetChildEntityValueListExcludingDeletedStatusItems(entity);
            ValidateForChildDeletion(entity, currentChildren);
            ICollection<ITypeFieldValueList> deletedChildren = OperationUtils.FindDeletedChildren(originalChildren, currentChildren);
            DeleteOrphanChildren(con, deletedChildren);
        }

        private void SaveForType(IEntity entity, Type type, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            if (entityInfo == null)
            {
                return;
            }

            ICollection<IRelation> dbRelations = entityInfo.Relations;
            foreach (IRelation relation in dbRelations)
            {
                if (relation.ReverseRelationship)
                {
                    continue;
                }
                if (IsProxyObject(entity, relation))
                {
                    continue;
                }
                ICollection<IEntity> childObjects = OperationUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {
                    if (relation.NonIdentifyingRelation)
                    {
                        foreach (RelationColumnMapping mapping in relation.TableColumnMappings)
                        {
                            SetParentRelationFieldsForNonIdentifyingRelations(entity,relation.RelatedObjectType, childObjects, mapping);
                        }
                    }
                }
            }

            ITypeFieldValueList fieldValues = OperationUtils.ExtractEntityTypeFieldValues(entity, type);
            if (entity.Status == EntityStatus.Unmodified)
            {
                //do nothing
            }
            else if (entity.Status == EntityStatus.New)
            {
                Insert(entity,fieldValues, type, con);
            }
            else if (entity.Status == EntityStatus.Modified)
            {
                if (Config.CheckVersion)
                {
                    if (!VersionValidated(entity, type, con))
                    {
                        throw new DataUpdatedFromAnotherSourceException(String.Format("The type {0} updated from another transaction", type));
                    }
                    OperationUtils.IncrementVersion(fieldValues);
                    SetValues(entity, fieldValues);
                }
                Update(entity, fieldValues, type, con);
            }
            else if (entity.Status == EntityStatus.Deleted)
            {
                Delete(fieldValues, type, con);
            }
            else
            {
                string message = String.Format("In-corret status for class {0}", type.FullName);
                throw new IncorrectStatusException(message);
            }

            fieldValues = OperationUtils.ExtractEntityTypeFieldValues(entity, type);
            IEntityContext entityContext = entity.Context;
            if (entityContext != null)
            {
                foreach (EntityFieldValue fieldValue in fieldValues.FieldValues)
                {
                    EntityFieldValue entityFieldValue = entityContext.ChangeTracker.GetFieldValue(fieldValue.Column.AttributeName);
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
            SessionUtils.AddToSession(entity, OperationUtils.ExtractEntityKeyValues(entity));

            foreach (IRelation relation in dbRelations)
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

                ICollection<IEntity> childObjects = OperationUtils.GetRelationEntities(entity, relation);
                if (childObjects != null)
                {

                    SetRelationObjectKeyValues(fieldValues, type,relation.RelatedObjectType, childObjects, relation);
                    foreach (IEntity fieldObject in childObjects)
                    {
                        IEntityFieldValueList childEntityKeyList = OperationUtils.ExtractEntityKeyValues(fieldObject);
                        if (SessionUtils.ExistsInSession(entity, childEntityKeyList))
                        {
                            continue;
                        }
                        SessionUtils.TransferSession(entity, fieldObject);
                        if (fieldObject.Status != EntityStatus.Deleted) //deleted items are already deleted
                        {
                            fieldObject.Persist(con);
                            SessionUtils.AddToSession(entity, childEntityKeyList);
                        }
                    }
                }
            }
        }

        private void Insert(IEntity entity,ITypeFieldValueList valueTypeList, Type entityType, IDbConnection con)
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
                IColumn column = fieldValue.Column;
                Object columnValue;

                if (column.ReadFromSequence
                        && column.SequenceGenerator != null)
                {
                    columnValue = column.SequenceGenerator.GetNextSequenceValue(con);
                    PropertyInfo setter = entityType.GetProperty(column.AttributeName);
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
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            if (Config.EnableStatistics)
            {
                Statistics.RegisterInsert(entityType);
            }
            cmd.ExecuteNonQuery();
            DbMgtUtility.Close(cmd);
        }

        private void Update(IEntity entity, ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
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

                keys = OperationUtils.ExtractEntityKeyValues(entity).FieldValues;
                ICollection<IColumn> keysAndModified = new List<IColumn>();
                foreach (EntityFieldValue fieldValue in values)
                {
                    keysAndModified.Add(fieldValue.Column);
                }
                foreach (EntityFieldValue fieldValue in keys)
                {
                    keysAndModified.Add(fieldValue.Column);
                }
                query = DbLayer.DataManipulate().CreateUpdateQuery(entityInfo.TableName, keysAndModified);
            }
            else
            {
                query = entityInfo.GetUpdateQuery(DbLayer);
                foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
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
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.Column);
            }

            foreach (EntityFieldValue fieldValue in keys)
            {
                if (showQuery)
                {
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(cmd, fieldValue.Value, ++count, fieldValue.Column);
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
            DbMgtUtility.Close(cmd);
        }

        private void Delete(ITypeFieldValueList valueTypeList, Type type, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            StringBuilder logSb = new StringBuilder();
            string query = entityInfo.GetDeleteQuery(DbLayer);
            IList<EntityFieldValue> keys = new List<EntityFieldValue>();

            foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
            {
                if (fieldValue.Column.Key)
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
                    logSb.Append(" ,").Append(fieldValue.Column.ColumnName).Append("=").Append(fieldValue.Value);
                }
                DbLayer.DataManipulate().SetToPreparedStatement(ps, fieldValue.Value, i + 1, fieldValue.Column);
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
            DbMgtUtility.Close(ps);
        }

        private static void SetRelationObjectKeyValues(ITypeFieldValueList valueTypeList, Type entityType,Type childEntityType
            , IEnumerable<IEntity> childObjects, IRelation relation)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);
            ICollection<IColumn> columns = entityInfo.Columns;
            foreach (RelationColumnMapping mapping in relation.TableColumnMappings)
            {
                IColumn matchColumn = OperationUtils.FindColumnByAttribute(columns, mapping.FromField);
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
            , IEnumerable<IEntity> childObjects, RelationColumnMapping mapping)
        {
            bool foundOnce = false;
            EntityInfo parentEntityInfo = CacheManager.GetEntityInfo(childEntityType);
            EntityInfo entityInfo = parentEntityInfo;

            while (entityInfo != null)
            {
                ICollection<IColumn> subLevelColumns = entityInfo.Columns;
                IColumn subLevelMatchedColumn = OperationUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (subLevelMatchedColumn != null)
                {
                    foundOnce = true;
                    PropertyInfo setter = parentEntityInfo.GetProperty(subLevelMatchedColumn);
                    foreach (IReadOnlyEntity dbObject in childObjects)
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

        private static void SetParentRelationFieldsForNonIdentifyingRelations(IEntity parentEntity,Type childEntityType
            , IEnumerable<IEntity> childObjects, RelationColumnMapping mapping)
        {
            IReadOnlyEntity firstObject = null;
            IEnumerator<IEntity> childEnumerator = childObjects.GetEnumerator();
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
                ICollection<IColumn> parentColumns = parentInfo.Columns;
                IColumn parentMatchedColumn = OperationUtils.FindColumnByAttribute(parentColumns, mapping.FromField);
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
                ICollection<IColumn> subLevelColumns = childInfo.Columns;
                IColumn childMatchedColumn = OperationUtils.FindColumnByAttribute(subLevelColumns, mapping.ToField);

                if (childMatchedColumn != null)
                {
                    foundOnce = true;
                    foreach (IReadOnlyEntity dbObject in childObjects)
                    {
                        ITypeFieldValueList fieldValueList = OperationUtils.ExtractEntityTypeFieldValues(dbObject, childInfo.EntityType);
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

        private static void ValidateForChildDeletion(IEntity currentEntity, IEnumerable<ITypeFieldValueList> currentChildren)
        {
            foreach (ITypeFieldValueList keyValueList in currentChildren)
            {
                EntityRelationFieldValueList entityRelationKeyValueList = (EntityRelationFieldValueList)keyValueList;
                if (entityRelationKeyValueList.Relation.DeleteRule == ReferentialRuleType.Restrict
                        && !entityRelationKeyValueList.Relation.ReverseRelationship
                        && currentEntity.Status == EntityStatus.Deleted)
                {
                    throw new IntegrityConstraintViolationException(String.Format("Cannot delete child object {0} as restrict constraint in place", keyValueList.Type.FullName));
                }
            }
        }

        private bool CheckForModification(IEntity entity,IDbConnection con, IEntityContext entityContext)
        {
            if (!entityContext.ChangeTracker.Valid)
            {
                FillChangeTrackerValues(entity, con, entityContext);
            }

            EntityInfo entityInfo = CacheManager.GetEntityInfo(entity);
            while (entityInfo != null)
            {
                ICollection<IColumn> subLevelColumns = entityInfo.Columns;
                foreach (IColumn subLevelColumn in subLevelColumns)
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

        private void FillChangeTrackerValues(IEntity entity, IDbConnection con, IEntityContext entityContext)
        {
            if (entity.Status == EntityStatus.New
                    || entity.Status == EntityStatus.Deleted)
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

                ICollection<IRelation> dbRelations = entityInfo.Relations;
                foreach (IRelation relation in dbRelations)
                {
                    ICollection<IReadOnlyEntity> children = ReadRelationChildrenFromDb(entity,entityInfo.EntityType,con,relation);
                    foreach (IReadOnlyEntity childEntity in children)
                    {
                        ITypeFieldValueList valueTypeList = OperationUtils.ExtractRelationKeyValues(childEntity,relation);
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
                    DbMgtUtility.Close(reader);
                    DbMgtUtility.Close(cmd);
                }
                
                if (recordExists)
                {
                    Delete(relationKeyValueList,relationKeyValueList.Type,con);
                }
            }
        }

        private bool VersionValidated(IReadOnlyEntity entity, Type type, IDbConnection con)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<IColumn> typeColumns = entityInfo.Columns;
            foreach (IColumn typeColumn in typeColumns)
            {
                if (typeColumn.ColumnType == ColumnType.Version)
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
		 	    typeColumns = new List<IColumn>();
		 	    foreach (EntityFieldValue fieldValue in modified)
		 	    {
		 	        typeColumns.Add(fieldValue.Column);
		 	    }
		 	}

            ITypeFieldValueList fieldValueList = ExtractCurrentRowValues(entity,type,con);
            if (fieldValueList == null)
            {
                return false;
            }
            foreach (IColumn typeColumn in typeColumns)
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

        private ICollection<EntityFieldValue> GetModifiedFieldValues(IReadOnlyEntity entity, Type type)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            ICollection<IColumn> typeColumns = entityInfo.Columns;
            ITypeFieldValueList currentValues = OperationUtils.ExtractEntityTypeFieldValues(entity,type);
            ICollection<EntityFieldValue> modifiedColumns = new  List<EntityFieldValue>();

            foreach (IColumn typeColumn in typeColumns)
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

        private Object ExtractCurrentVersionValue(IReadOnlyEntity entity, IColumn versionColumn, Type type
            , IDbConnection con)
        {
            Object versionValue = null;

            ITypeFieldValueList keyFieldValueList = OperationUtils.ExtractEntityTypeKeyValues(entity, type);
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
                DbMgtUtility.Close(reader);
                DbMgtUtility.Close(cmd);  
            }
            return versionValue;
        }

        private ITypeFieldValueList ExtractCurrentRowValues(IReadOnlyEntity entity, Type type, IDbConnection con)
        {
            ITypeFieldValueList fieldValueList = null;

            ITypeFieldValueList keyFieldValueList = OperationUtils.ExtractEntityTypeKeyValues(entity, type);
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
                DbMgtUtility.Close(reader);
                DbMgtUtility.Close(cmd);   
            }
            return fieldValueList;
        }
    }
}
