﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.exceptions.common;
using dbgate.ermanagement.impl.utils;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace dbgate.ermanagement.caches.impl
{
    public class EntityInfoCache : IEntityInfoCache
    {
        private static readonly Dictionary<Type, EntityInfo> Cache = new Dictionary<Type, EntityInfo>();
        private IErLayerConfig _config;

        public EntityInfoCache(IErLayerConfig config)
        {
            _config = config;
        }

        public EntityInfo GetEntityInfo(Type entityType)
        {
            try
            {
                if (!Cache.ContainsKey(entityType))
                {
                    Register(entityType);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(_config.LoggerName).Fatal(ex.Message, ex);
            }
            return Cache[entityType];
        }

        public EntityInfo GetEntityInfo(IRoDbClass entity)
        {
            var entityType = entity.GetType();
            try
            {
                if (!Cache.ContainsKey(entityType))
                {
                    Register(entityType);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(_config.LoggerName).Fatal(ex.Message, ex);
            }
            return Cache[entityType];
        }

        public void Register(Type subType, string tableName, ICollection<IField> fields)
        {
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(subType,
                                                                                     new Type[]
                                                                                         {typeof (IServerRoDbClass)});
            Type immediateSuper = typeList.Length > 1 ? typeList[1] : null;

            var subEntityInfo = new EntityInfo(subType);
            subEntityInfo.SetFields(fields);
            subEntityInfo.TableName = tableName;

            if (immediateSuper != null
                && Cache.ContainsKey(immediateSuper))
            {
                EntityInfo immediateSuperEntityInfo = Cache[immediateSuper];
                subEntityInfo.SuperEntityInfo = immediateSuperEntityInfo;
            }

            lock (Cache)
            {
                Cache.Add(subType, subEntityInfo);
            }
        }

        public void Register(Type entityType)
        {
            if (Cache.ContainsKey(entityType))
            {
                return;
            }
            var extracted = ExtractTableAndFieldInfo(entityType);
            lock (Cache)
            {
                foreach (Type regType in extracted.Keys)
                {
                    Cache.Add(regType, extracted[regType]);
                }
            }
        }

        public void Clear()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
        }

        private Dictionary<Type, EntityInfo> ExtractTableAndFieldInfo(Type subType)
        {
            var entityInfoMap = new Dictionary<Type, EntityInfo>();

            EntityInfo subEntity = null;
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(subType,
                                                                                     new[] {typeof (IServerRoDbClass)});
            foreach (Type regType in typeList)
            {
                if (Cache.ContainsKey(regType))
                {
                    subEntity.SuperEntityInfo = Cache[regType];
                    continue;
                }

                String tableName = GetTableName(regType, subType);
                ICollection<IField> fields = GetAllFields(regType, subType);

                if (tableName != null || fields.Count > 0)
                {
                    var entityInfo = new EntityInfo(regType);
                    entityInfo.SetFields(fields);
                    entityInfo.TableName = tableName;
                    if (subEntity != null)
                    {
                        subEntity.SuperEntityInfo = entityInfo;
                    }
                    entityInfoMap.Add(regType, entityInfo);
                    subEntity = entityInfo;
                }
            }
            return entityInfoMap;
        }

        private static String GetTableName(Type regType, Type subType)
        {
            String tableName = GetTableNameIfManagedClass(regType, subType);
            if (tableName == null)
            {
                object[] annotations = regType.GetCustomAttributes(false);
                foreach (object annotation in annotations)
                {
                    if (annotation is DbTableInfo)
                    {
                        var tableInfo = (DbTableInfo) annotation;
                        tableName = tableInfo.TableName;
                        break;
                    }
                }
            }
            return tableName;
        }

        private static string GetTableNameIfManagedClass(Type regType, Type subType)
        {
            if (ReflectionUtils.IsImplementInterface(regType, typeof (IManagedDbClass)))
            {
                try
                {
                    var managedDBClass = (IManagedDbClass) Activator.CreateInstance(subType);
                    return managedDBClass.TableNames.ContainsKey(regType) ? managedDBClass.TableNames[regType] : null;
                }
                catch (Exception e)
                {
                    throw new EntityRegistrationException(
                        String.Format("Could not register type {0}", regType.FullName), e);
                }
            }
            return null;
        }

        private static ICollection<IField> GetAllFields(Type regType, Type subType)
        {
            List<IField> fields = GetFieldsIfManagedClass(regType, subType);
            Type[] superTypes = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(regType,
                                                                                       new Type[]
                                                                                           {typeof (IServerRoDbClass)});

            for (int i = 0; i < superTypes.Length; i++)
            {
                Type superType = superTypes[i];
                fields.AddRange(GetAllFields(superType, i > 0));
            }

            return fields;
        }

        private static List<IField> GetFieldsIfManagedClass(Type regType, Type subType)
        {
            if (ReflectionUtils.IsImplementInterface(regType, typeof (IManagedDbClass)))
            {
                try
                {
                    var managedDBClass = (IManagedDbClass) Activator.CreateInstance(subType);
                    return GetFieldsForManagedClass(managedDBClass, regType);
                }
                catch (Exception e)
                {
                    throw new EntityRegistrationException(
                        String.Format("Could not register type {0}", regType.FullName), e);
                }
            }
            return new List<IField>();
        }

        private static List<IField> GetFieldsForManagedClass(IManagedRoDbClass entity, Type type)
        {
            var fields = new List<IField>();

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(type,
                                                                                     new[] {typeof (IServerRoDbClass)});
            foreach (Type targetType in typeList)
            {
                ICollection<IField> targetTypeFields = entity.FieldInfo.ContainsKey(targetType)
                                                           ? entity.FieldInfo[targetType]
                                                           : null;
                if (targetType == type && targetTypeFields != null)
                {
                    fields.AddRange(targetTypeFields);
                }
                else if (targetTypeFields != null)
                {
                    foreach (IField field in targetTypeFields)
                    {
                        var dbColumn = field as IDbColumn;
                        if (dbColumn != null && dbColumn.SubClassCommonColumn)
                        {
                            fields.Add(dbColumn);
                        }
                    }
                }
            }

            return fields;
        }

        private static List<IField> GetAllFields(Type entityType, bool superClass)
        {
            var fields = new List<IField>();

            PropertyInfo[] dbClassFields = entityType.GetProperties();
            foreach (PropertyInfo propertyInfo in dbClassFields)
            {
                if (propertyInfo.DeclaringType != entityType)
                {
                    continue;
                }

                object[] annotations = propertyInfo.GetCustomAttributes(false);
                foreach (object annotation in annotations)
                {
                    if (annotation is DbColumnInfo)
                    {
                        var dbColumnInfo = (DbColumnInfo) annotation;
                        IDbColumn column = CreateColumnMapping(propertyInfo, dbColumnInfo);
                        if (superClass)
                        {
                            if (column.SubClassCommonColumn)
                            {
                                fields.Add(column);
                            }
                        }
                        else
                        {
                            fields.Add(column);
                        }
                    }
                    else if (annotation is ForeignKeyInfo)
                    {
                        if (superClass)
                        {
                            continue;
                        }
                        var foreignKeyInfo = (ForeignKeyInfo) annotation;
                        IDbRelation relation = CreateForeignKeyMapping(propertyInfo, foreignKeyInfo);
                        fields.Add(relation);
                    }
                }
            }

            return fields;
        }

        private static IDbColumn CreateColumnMapping(PropertyInfo propertyInfo, DbColumnInfo dbColumnInfo)
        {
            IDbColumn column = new DefaultDbColumn(propertyInfo.Name, dbColumnInfo.ColumnType, dbColumnInfo.Nullable);
            if (dbColumnInfo.ColumnName != null
                && dbColumnInfo.ColumnName.Trim().Length > 0)
            {
                column.ColumnName = dbColumnInfo.ColumnName;
            }
            column.Key = dbColumnInfo.Key;
            column.Size = dbColumnInfo.Size;
            column.SubClassCommonColumn = dbColumnInfo.SubClassCommonColumn;
            column.ReadFromSequence = dbColumnInfo.ReadFromSequence;
            if (column.ReadFromSequence)
            {
                try
                {
                    Type sequenceType = Type.GetType(dbColumnInfo.SequenceGeneratorClassName);
                    column.SequenceGenerator = (ISequenceGenerator) Activator.CreateInstance(sequenceType);
                }
                catch (Exception e)
                {
                    throw new SequenceGeneratorInitializationException(e.Message, e);
                }
            }
            return column;
        }

        private static IDbRelation CreateForeignKeyMapping(PropertyInfo propertyInfo, ForeignKeyInfo foreignKeyInfo)
        {
            var objectMappings = new DbRelationColumnMapping[foreignKeyInfo.FromColumnMappings.Length];
            string[] fromColumnMappings = foreignKeyInfo.FromColumnMappings;
            string[] toColumnMappings = foreignKeyInfo.ToColumnMappings;

            if (fromColumnMappings.Length != toColumnMappings.Length)
            {
                LogManager.GetLogger(typeof (EntityInfoCache)).Fatal(
                    "incorrect relation definition, no of from columns should ne equal to no of to columns");
                throw new IncorrectFieldDefinitionException(
                    "incorrect relation definition, no of from columns should ne equal to no of to columns");
            }

            for (int i = 0; i < fromColumnMappings.Length; i++)
            {
                string fromMapping = fromColumnMappings[i];
                string toMapping = toColumnMappings[i];
                objectMappings[i] = new DbRelationColumnMapping(fromMapping, toMapping);
            }

            IDbRelation relation = new DefaultDbRelation(propertyInfo.Name, foreignKeyInfo.Name
                                                         , foreignKeyInfo.RelatedOjectType, objectMappings
                                                         , foreignKeyInfo.UpdateRule, foreignKeyInfo.DeleteRule
                                                         , foreignKeyInfo.ReverseRelation
                                                         , foreignKeyInfo.NonIdentifyingRelation, foreignKeyInfo.Lazy);

            return relation;
        }
    }
}
