using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.Context;
using DbGate.Context.Impl;

namespace DbGate.ErManagement.ErMapper.Utils
{
    public class OperationUtils
    {
        public static ICollection<IEntity> GetRelationEntities(IReadOnlyEntity rootEntity, IRelation relation)
        {
            var entityInfo = CacheManager.GetEntityInfo(rootEntity);
            var property = entityInfo.GetProperty(relation.AttributeName);
            var value = ReflectionUtils.GetValue(entityInfo.EntityType, property.Name, rootEntity);

            ICollection<IEntity> treeEntities = new List<IEntity>();
            if (value is ICollection)
            {
                var collection = (ICollection) value;
                foreach (var o in collection)
                {
                    if (o is IEntity
                        && ReflectionUtils.IsSubClassOf(o.GetType(),relation.RelatedObjectType))
                    {
                        treeEntities.Add((IEntity) o);
                    }
                }
            }
            else if (value is IEntity
                     && ReflectionUtils.IsSubClassOf(value.GetType(),relation.RelatedObjectType))
            {
                treeEntities.Add((IEntity) value);
            }
            return treeEntities;
        }

        public static IEntityFieldValueList ExtractEntityKeyValues(IReadOnlyEntity entity)
        {
            EntityFieldValueList valueList = null;
            if (entity is IEntity)
            {
                valueList = new EntityFieldValueList(entity);
                var entityDbClass = (IEntity) entity;
                var extractedValues = ExtractValues(entityDbClass, true, entity.GetType());
                foreach (var entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractEntityTypeFieldValues(IReadOnlyEntity entity,Type type) 
        {
            EntityTypeFieldValueList valueList = null;
            if (entity is IEntity)
            {
                valueList = new EntityTypeFieldValueList(type);
                var entityDbClass = (IEntity) entity;
                var extractedValues = ExtractValues(entityDbClass,false,type);
                foreach (var entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractEntityTypeKeyValues(IReadOnlyEntity entity,Type type)
        {
            EntityTypeFieldValueList valueList = null;
            if (entity is IEntity)
            {
                valueList = new EntityTypeFieldValueList(type);
                var entityDbClass = (IEntity) entity;
                var extractedValues = ExtractValues(entityDbClass,true,type);
                foreach (var entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractRelationKeyValues(IReadOnlyEntity child,IRelation relation)
        {
            EntityRelationFieldValueList valueList = null;
            if (child is IEntity)
            {
                valueList = new EntityRelationFieldValueList(relation);
                var childDbClass = (IEntity) child;
                var extractedValues = ExtractValues(childDbClass,true,null);
                foreach (var entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        private static ICollection<EntityFieldValue> ExtractValues(IEntity entity,bool key,Type typeToLoad)
        {
            ICollection<EntityFieldValue> entityFieldValues = new List<EntityFieldValue>();
            var parentEntityInfo = CacheManager.GetEntityInfo(entity);
            var entityInfo = parentEntityInfo;

            while (entityInfo != null)
            {
                if (typeToLoad != null && typeToLoad != entityInfo.EntityType)
                {
                    entityInfo = entityInfo.SuperEntityInfo;
                    continue;
                }
                var subLevelColumns = entityInfo.Columns;
                foreach (var subLevelColumn in subLevelColumns)
                {
                    if (!key || (subLevelColumn.Key && key))
                    {
                        if (AlreadyHasTheColumnAdded(entityFieldValues, subLevelColumn))
                        {
                            continue;
                        }
                        
                        var relationColumnInfo = entityInfo.FindRelationColumnInfo(subLevelColumn.AttributeName);
                        if (relationColumnInfo != null)
                        {
                            var relationEntities = GetRelationEntities(entity,relationColumnInfo.Relation);
                            if (relationEntities.Count > 0)
                            {
                                foreach (var relationEntity in relationEntities)
                                {
                                    var keyValueList = ExtractEntityKeyValues(relationEntity);
                                    var fieldValue = keyValueList.GetFieldValue(relationColumnInfo.Mapping.ToField);
                                    entityFieldValues.Add(new EntityFieldValue(fieldValue.Value,subLevelColumn));
                                }
                            }
                            else
                            {
                                entityFieldValues.Add(new EntityFieldValue(null,subLevelColumn));
                            }
                        }
                        else
                        {
                            var getter = entityInfo.GetProperty(subLevelColumn.AttributeName);
                            var value = ReflectionUtils.GetValue(entityInfo.EntityType, getter.Name, entity);

                            entityFieldValues.Add(new EntityFieldValue(value, subLevelColumn));
                        }
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
            return entityFieldValues;
        }

        private static bool AlreadyHasTheColumnAdded(ICollection<EntityFieldValue> entityFieldValues,IColumn column)
        {
            foreach (var fieldValue in entityFieldValues)
            {
                if (fieldValue.Column.AttributeName.Equals(column.AttributeName))
                {
                    return true;
                }
            }
            return false;
        }

        public static ICollection<ITypeFieldValueList> FindDeletedChildren(IEnumerable<ITypeFieldValueList> startListRelation
                                                                           ,ICollection<ITypeFieldValueList> currentListRelation)
        {
            ICollection<ITypeFieldValueList> deletedListRelation = new List<ITypeFieldValueList>();

            foreach (var keyValueList in startListRelation)
            {
                var found  = false;
                foreach (var relationKeyValueListCurrent in currentListRelation)
                {
                    found = IsTypeKeyEquals(keyValueList, relationKeyValueListCurrent);
                    if (found)
                    {
                        break;
                    }
                }
                if (!found)
                {
                    deletedListRelation.Add(keyValueList);
                }
            }

            return deletedListRelation;
        }

        public static bool IsTypeKeyEquals(ITypeFieldValueList item1, ITypeFieldValueList item2)
        {
            return item1.Type == item2.Type && IsValueKeyEquals(item1, item2);
        }

        public static bool IsEntityKeyEquals(IEntityFieldValueList item1, IEntityFieldValueList item2)
        {
            return item1.Entity == item2.Entity && IsValueKeyEquals(item1, item2);
        }

        private static bool IsValueKeyEquals(IFieldValueList item1, IFieldValueList item2)
        {
            foreach (var fieldValue1 in item1.FieldValues)
            {
                var found  = false;
                foreach (var fieldValue2 in item2.FieldValues)
                {
                    if (fieldValue1.Column.AttributeName.Equals(fieldValue2.Column.AttributeName))
                    {
                        found = fieldValue1.Value == fieldValue2.Value
                                || (fieldValue1.Value != null && fieldValue1.Value.Equals(fieldValue2.Value));
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        public static void IncrementVersion(ITypeFieldValueList fieldValues)
        {
            foreach (var fieldValue in fieldValues.FieldValues)
            {
                if (fieldValue.Column.ColumnType == ColumnType.Version)
                {
                    var version = int.Parse(fieldValue.Value.ToString());
                    version++;
                    fieldValue.Value = version;
                    break;
                }
            }
        }
    }
}
