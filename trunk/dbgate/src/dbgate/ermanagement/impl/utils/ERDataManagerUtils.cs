using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;

namespace dbgate.ermanagement.impl.utils
{
    public class ErDataManagerUtils
    {
        public static ICollection<IServerDbClass> GetRelationEntities(IServerRoDbClass parent, IDbRelation relation)
        {
            PropertyInfo property = CacheManager.MethodCache.GetProperty(parent.GetType(),relation.AttributeName);
            object value = property.GetValue(parent,null);

            ICollection<IServerDbClass> fieldObjects = new List<IServerDbClass>();
            if (value is ICollection)
            {
                ICollection collection = (ICollection) value;
                foreach (object o in collection)
                {
                    if (o is IServerDbClass
                        && ReflectionUtils.IsSubClassOf(o.GetType(),relation.RelatedObjectType))
                    {
                        fieldObjects.Add((IServerDbClass) o);
                    }
                }
            }
            else if (value is IServerDbClass
                     && ReflectionUtils.IsSubClassOf(value.GetType(),relation.RelatedObjectType))
            {
                fieldObjects.Add((IServerDbClass) value);
            }
            return fieldObjects;
        }

        public static IEntityFieldValueList ExtractEntityKeyValues(IServerRoDbClass entity)
        {
            EntityFieldValueList valueList = null;
            if (entity is IServerDbClass)
            {
                valueList = new EntityFieldValueList(entity);
                IServerDbClass entityDbClass = (IServerDbClass) entity;
                ICollection<EntityFieldValue> extractedValues = ExtractValues(entityDbClass, true, entity.GetType());
                foreach (EntityFieldValue entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractTypeFieldValues(IServerRoDbClass entity,Type type) 
        {
            EntityTypeFieldValueList valueList = null;
            if (entity is IServerDbClass)
            {
                valueList = new EntityTypeFieldValueList(type);
                IServerDbClass entityDbClass = (IServerDbClass) entity;
                ICollection<EntityFieldValue> extractedValues = ExtractValues(entityDbClass,false,type);
                foreach (EntityFieldValue entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractTypeKeyValues(IServerRoDbClass entity,Type type)
        {
            EntityTypeFieldValueList valueList = null;
            if (entity is IServerDbClass)
            {
                valueList = new EntityTypeFieldValueList(type);
                IServerDbClass entityDbClass = (IServerDbClass) entity;
                ICollection<EntityFieldValue> extractedValues = ExtractValues(entityDbClass,true,type);
                foreach (EntityFieldValue entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        public static ITypeFieldValueList ExtractRelationKeyValues(IServerRoDbClass child,IDbRelation relation)
        {
            EntityRelationFieldValueList valueList = null;
            if (child is IServerDbClass)
            {
                valueList = new EntityRelationFieldValueList(relation);
                IServerDbClass childDbClass = (IServerDbClass) child;
                ICollection<EntityFieldValue> extractedValues = ExtractValues(childDbClass,true,null);
                foreach (EntityFieldValue entityFieldValue in extractedValues)
                {
                    valueList.FieldValues.Add(entityFieldValue);
                }
            }
            return valueList;
        }

        private static ICollection<EntityFieldValue> ExtractValues(IServerDbClass serverDBClass,bool key,Type typeToLoad)
        {
            ICollection<EntityFieldValue> entityFieldValues = new List<EntityFieldValue>();

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(serverDBClass.GetType(),new Type[]{typeof(IServerDbClass)});
            foreach (Type type in typeList)
            {
                if (typeToLoad != null && typeToLoad != type)
                {
                    continue;
                }
                ICollection<IDbColumn> subLevelColumns = CacheManager.FieldCache.GetDbColumns(type);
                foreach (IDbColumn subLevelColumn in subLevelColumns)
                {
                    if (!key || (subLevelColumn.Key && key))
                    {
                        PropertyInfo getter = CacheManager.MethodCache.GetProperty(serverDBClass.GetType(), subLevelColumn.AttributeName);
                        Object value = getter.GetValue(serverDBClass,null);

                        entityFieldValues.Add(new EntityFieldValue(value,subLevelColumn));
                    }
                }
            }
            return entityFieldValues;
        }

        public static ICollection<ITypeFieldValueList> FindDeletedChildren(ICollection<ITypeFieldValueList> startListRelation
                                                                           ,ICollection<ITypeFieldValueList> currentListRelation)
        {
            ICollection<ITypeFieldValueList> deletedListRelation = new List<ITypeFieldValueList>();

            foreach (ITypeFieldValueList keyValueList in startListRelation)
            {
                bool found  = false;
                foreach (ITypeFieldValueList relationKeyValueListCurrent in currentListRelation)
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
            foreach (EntityFieldValue fieldValue1 in item1.FieldValues)
            {
                bool found  = false;
                foreach (EntityFieldValue fieldValue2 in item2.FieldValues)
                {
                    if (fieldValue1.DbColumn.AttributeName.Equals(fieldValue2.DbColumn.AttributeName))
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

        public static IDbColumn FindColumnByAttribute(ICollection<IDbColumn> columns,String attribute)
        {
            foreach (IDbColumn column in columns)
            {
                if (column.AttributeName.Equals(attribute,StringComparison.InvariantCultureIgnoreCase))
                {
                    return column;
                }
            }
            return null;
        }

        public static void Reverse(Type[] types)
        {
            for (int left = 0, right = types.Length - 1; left < right; left++, right--)
            {
                Type temp = types[left];
                types[left]  = types[right];
                types[right] = temp;
            }
        }

        public static void IncrementVersion(ITypeFieldValueList fieldValues)
        {
            foreach (EntityFieldValue fieldValue in fieldValues.FieldValues)
            {
                if (fieldValue.DbColumn.ColumnType == DbColumnType.Version)
                {
                    int version = int.Parse(fieldValue.Value.ToString());
                    version++;
                    fieldValue.Value = version;
                    break;
                }
            }
        }

        public static void RegisterType(Type entityType) 
        {
            CacheManager.TableCache.Register(entityType);
            CacheManager.FieldCache.Register(entityType);
        }
    }
}
