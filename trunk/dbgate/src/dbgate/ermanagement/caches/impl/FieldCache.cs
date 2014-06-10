using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.utils;
using Castle.Core.Internal;

namespace dbgate.ermanagement.caches.impl
{
    public class FieldCache : IFieldCache
    {
        private static readonly Dictionary<string, ICollection<IField>> ColumnCache = new Dictionary<string, ICollection<IField>>();

        public ICollection<IField> GetFields(Type entityType)
        {
            ICollection<IField> retFields;
            string cacheKey = GetCacheKey(entityType);

            if (ColumnCache.ContainsKey(cacheKey))
            {
                retFields = ColumnCache[cacheKey];
            }
            else
            {
                throw new FieldCacheMissException(String.Format("No cache entry found for {0}", entityType.FullName));  
            }
            return retFields;
        }

        public ICollection<IDbColumn> GetDbColumns(Type entityType)
        {
            return GetDbColumns(GetFields(entityType),false);
        }

        public ICollection<IDbColumn> GetKeys(Type type)
        {
            return GetDbColumns(GetFields(type),true);
        }

        private static ICollection<IDbColumn> GetDbColumns(ICollection<IField> fields,bool keysOnly)
        {
            IList<IDbColumn> columns = new List<IDbColumn>();
            foreach (IField field in fields)
            {
                if (field is IDbColumn)
                {
                    var dbColumn = (IDbColumn) field;
                    if ((keysOnly && dbColumn.Key) || !keysOnly)
                    {
                        columns.Add(dbColumn);
                    }
                }
            }
            return columns;
        }

        public ICollection<IDbRelation> GetDbRelations(Type entityType)
        {
            return GetDbRelations(GetFields(entityType));
        }

        private static ICollection<IDbRelation> GetDbRelations(ICollection<IField> fields)
        {
            IList<IDbRelation> relations = new List<IDbRelation>();
            foreach (IField field in fields)
            {
                if (field is IDbRelation)
                {
                    var dbRelation = (IDbRelation)field;
                    relations.Add(dbRelation);
                }
            }
            return relations;
        }

        public void Register(Type entityType, ICollection<IField> fields)
        {
            string cacheKey = GetCacheKey(entityType);
            lock (ColumnCache)
            {
                ColumnCache.Add(cacheKey, fields);
            }
        }

        public void Register(Type entityType) 
        {
            if (ColumnCache.ContainsKey(GetCacheKey(entityType)))
            {
                return;
            }
            
            IManagedRoDbClass managedRoDbClass = null;
            if (ReflectionUtils.IsImplementInterface(entityType,typeof(IManagedRoDbClass)))
            {
                try
                {
                    managedRoDbClass = (IManagedRoDbClass)Activator.CreateInstance(entityType);
                }
                catch (Exception e)
                {
                    throw  new EntityRegistrationException(String.Format("Could not register type {0}",entityType.FullName),e);
                }
            }


            var tempStore = new Dictionary<string, ICollection<IField>>();
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(entityType,new Type[]{typeof(IServerRoDbClass)});
            foreach (Type regType in typeList)
            {
                string cacheKey = GetCacheKey(regType);
                if (ColumnCache.ContainsKey(cacheKey))
                {
                    continue;
                }

                ICollection<IField> extractedFields = DbClassAttributeExtractionUtils.GetAllFields(regType);
                ICollection<IField> managedFields = managedRoDbClass != null
                        ? GetFieldsForManagedClass(managedRoDbClass,regType)
                        : new List<IField>();

                if (managedFields != null)
                {
                    managedFields.ForEach(extractedFields.Add);
                }
                tempStore.Add(cacheKey,extractedFields );
            }

            if (tempStore.Count == 0)
            {
                throw new EntityRegistrationException(String.Format("Can't find list of fields for type {0}",entityType.FullName));
            }

            lock (ColumnCache)
            {
                foreach (string cacheKey in tempStore.Keys)
                {
                    ColumnCache.Add(cacheKey, tempStore[cacheKey]);
                }
            }
        }

        private ICollection<IField> GetFieldsForManagedClass(IManagedRoDbClass entity,Type entityType)
        {
            var fields = new List<IField>();

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(entityType,new[]{typeof(IServerRoDbClass)});
            foreach (Type targetType in typeList)
            {
                ICollection<IField> targetTypeFields = entity.FieldInfo.ContainsKey(targetType)
                                                           ? entity.FieldInfo[targetType]
                                                           : null;
                if (targetType == entityType && targetTypeFields != null)
                {
                    fields.AddRange(targetTypeFields);
                }
                else if (targetTypeFields != null)
                {
                    foreach (IField field in targetTypeFields)
                    {
                        if (field is IDbColumn)
                        {
                            var column = (IDbColumn) field;
                            if (column.SubClassCommonColumn)
                            {
                                fields.Add(column);
                            }
                        }
                    }
                }
            }

            return fields;
        }

        public void Clear()
        {
            ColumnCache.Clear();
        }

        private static string GetCacheKey(Type type)
        {
            return type.FullName;
        }
    }
}