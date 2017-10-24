using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exceptions;
using DbGate.Exceptions.Common;
using log4net;

namespace DbGate.Caches.Impl
{
    public class EntityInfoCache : IEntityInfoCache
    {
        private static readonly Dictionary<Type, EntityInfo> Cache = new Dictionary<Type, EntityInfo>();
        private IDbGateConfig _config;

        public EntityInfoCache(IDbGateConfig config)
        {
            _config = config;
        }

        public EntityInfo GetEntityInfo(IReadOnlyClientEntity entity)
        {
            var entityType = entity.GetType();
            return GetEntityInfo(entityType);
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
                Logger.GetLogger(_config.LoggerName).Fatal(ex.Message, ex);
            }
            return Cache[entityType];
        }
        
        public IList<IRelation> GetReversedRelationships(Type entityType)
        {
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(entityType,
                new Type[]
                    {typeof (IReadOnlyEntity)});

            return Cache.SelectMany(c => c.Value.Relations)
                .Where(r => !r.ReverseRelationship &&
                    !r.NonIdentifyingRelation &&
                    typeList.Contains(r.RelatedObjectType))
                .Select(r => r).ToList();
        }

        public void Register(Type type, ITable table, ICollection<IField> fields)
        {
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(type,
                                                                                     new Type[]
                                                                                         {typeof (IReadOnlyEntity)});
            Type immediateSuper = typeList.Length > 1 ? typeList[1] : null;

            var subEntityInfo = new EntityInfo(type);
            subEntityInfo.SetFields(fields);
            subEntityInfo.TableInfo = table;

            if (immediateSuper != null
                && Cache.ContainsKey(immediateSuper))
            {
                EntityInfo immediateSuperEntityInfo = Cache[immediateSuper];
                subEntityInfo.SuperEntityInfo = immediateSuperEntityInfo;
                immediateSuperEntityInfo.AddSubEntityInfo(subEntityInfo);
            }

            lock (Cache)
            {
                Cache.Add(type, subEntityInfo);
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
                                                                                     new[] {typeof (IReadOnlyEntity)});
            foreach (Type regType in typeList)
            {
                if (subEntity != null && Cache.ContainsKey(regType))
                {
                    EntityInfo superEntityInfo = Cache[regType];
                    subEntity.SuperEntityInfo = superEntityInfo;
                    superEntityInfo.AddSubEntityInfo(subEntity);
                    continue;
                }

                ITable tableInfo = GetTableInfo(regType, subType);
                ICollection<IField> fields = GetAllFields(regType, subType);

                if (tableInfo != null || fields.Count > 0)
                {
                    var entityInfo = new EntityInfo(regType);
                    entityInfo.SetFields(fields);
                    entityInfo.TableInfo = tableInfo;
                    if (subEntity != null)
                    {
                        subEntity.SuperEntityInfo = entityInfo;
                        entityInfo.AddSubEntityInfo(subEntity);
                    }
                    entityInfoMap.Add(regType, entityInfo);
                    subEntity = entityInfo;
                }
            }
            return entityInfoMap;
        }

        private ITable GetTableInfo(Type regType, Type subType)
        {
            ITable table = GetTableInfoIfManagedClass(regType, subType);
            if (table == null)
            {
                object[] attributes = regType.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is TableInfo)
                    {
                        var tableInfo = (TableInfo) attribute;
                        TableInfo annotatedTableInfo = (TableInfo) attribute;
                        table = new DefaultTable(annotatedTableInfo.TableName
                            ,annotatedTableInfo.UpdateStrategy
                            ,annotatedTableInfo.VerifyOnWriteStrategy
                            ,annotatedTableInfo.DirtyCheckStrategy);
                    }
                }
            }
            UpdateToDefaultStrategiesIfNoneDefined(table);
            return table;
        }

        private void UpdateToDefaultStrategiesIfNoneDefined(ITable table)
        {
            if (table != null)
            {
                if (table.DirtyCheckStrategy == DirtyCheckStrategy.Default)
                    table.DirtyCheckStrategy = _config.DirtyCheckStrategy;

                if (table.UpdateStrategy == UpdateStrategy.Default)
                    table.UpdateStrategy = _config.UpdateStrategy;

                if (table.VerifyOnWriteStrategy == VerifyOnWriteStrategy.Default)
                    table.VerifyOnWriteStrategy = _config.VerifyOnWriteStrategy;
            }
        }

        private static ITable GetTableInfoIfManagedClass(Type regType, Type subType)
        {
            if (ReflectionUtils.IsImplementInterface(regType, typeof (IManagedEntity)))
            {
                try
                {
                    var managedDBClass = (IManagedEntity) Activator.CreateInstance(subType);
                    return managedDBClass.TableInfo.ContainsKey(regType) ? managedDBClass.TableInfo[regType] : null;
                }
                catch (Exception e)
                {
                    throw new EntityRegistrationException(
                        String.Format("Could not register type {0}", regType.FullName), e);
                }
            }
            return null;
        }

        private ICollection<IField> GetAllFields(Type regType, Type subType)
        {
            List<IField> fields = GetFieldsIfManagedClass(regType, subType);
            Type[] superTypes = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(regType,
                                                                                       new Type[]
                                                                                           {typeof (IReadOnlyEntity)});

            for (int i = 0; i < superTypes.Length; i++)
            {
                Type superType = superTypes[i];
                fields.AddRange(GetAllFields(superType, i > 0));
            }

            foreach (IField field in fields)
	        {
	            if (field is IRelation)
	            {
	                IRelation relation = (IRelation) field;
	
	                if (relation.FetchStrategy == FetchStrategy.Default)
	                    relation.FetchStrategy = _config.FetchStrategy;
	            }
	        }

            return fields;
        }

        private static List<IField> GetFieldsIfManagedClass(Type regType, Type subType)
        {
            if (ReflectionUtils.IsImplementInterface(regType, typeof (IManagedEntity)))
            {
                try
                {
                    var managedDBClass = (IManagedEntity) Activator.CreateInstance(subType);
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

        private static List<IField> GetFieldsForManagedClass(IManagedReadOnlyEntity entity, Type type)
        {
            var fields = new List<IField>();

            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(type,
                                                                                     new[] {typeof (IReadOnlyEntity)});
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
                        var dbColumn = field as IColumn;
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

                object[] attributes = propertyInfo.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is ColumnInfo)
                    {
                        var dbColumnInfo = (ColumnInfo) attribute;
                        if (superClass && !dbColumnInfo.SubClassCommonColumn)
                        {
                            continue;
                        }
                        IColumn column = CreateColumnMapping(propertyInfo, dbColumnInfo);
                        fields.Add(column);
                    }
                    else
                    {
                        if (superClass)
                        {
                            continue;
                        }
                        if (attribute is ForeignKeyInfo)
                        {
                            var foreignKeyInfo = (ForeignKeyInfo) attribute;

                            var relation = CreateForeignKeyMapping(entityType,
                                propertyInfo,
                                foreignKeyInfo);

                            fields.Add(relation);
                        }
                    }
                }
            }

            return fields;
        }

        private static IColumn CreateColumnMapping(PropertyInfo propertyInfo, ColumnInfo columnInfo)
        {
            IColumn column = new DefaultColumn(propertyInfo.Name, columnInfo.ColumnType, columnInfo.Nullable);
            if (columnInfo.ColumnName != null
                && columnInfo.ColumnName.Trim().Length > 0)
            {
                column.ColumnName = columnInfo.ColumnName;
            }
            column.Key = columnInfo.Key;
            column.Size = columnInfo.Size;
            column.SubClassCommonColumn = columnInfo.SubClassCommonColumn;
            column.ReadFromSequence = columnInfo.ReadFromSequence;
            if (column.ReadFromSequence)
            {
                try
                {
                    column.SequenceGenerator =
                        (ISequenceGenerator) Activator.CreateInstance(columnInfo.SequenceGeneratorType);
                }
                catch (Exception e)
                {
                    throw new SequenceGeneratorInitializationException(e.Message, e);
                }
            }
            return column;
        }

        private static IRelation CreateForeignKeyMapping(Type entityType,
            PropertyInfo propertyInfo,
            ForeignKeyInfo foreignKeyInfo)
        {
            var objectMappings = CreateForeignKeyColumnMappings(foreignKeyInfo);

            IRelation relation = new DefaultRelation(propertyInfo.Name,
                foreignKeyInfo.Name,
                entityType,
                foreignKeyInfo.RelatedOjectType,
                objectMappings,
                foreignKeyInfo.UpdateRule,
                foreignKeyInfo.DeleteRule,
                foreignKeyInfo.ReverseRelation,
                foreignKeyInfo.NonIdentifyingRelation,
                foreignKeyInfo.FetchStrategy,
                foreignKeyInfo.Nullable);

            return relation;
        }

        private static RelationColumnMapping[] CreateForeignKeyColumnMappings(ForeignKeyInfo foreignKeyInfo)
        {
            var objectMappings = new RelationColumnMapping[foreignKeyInfo.FromFieldMappings.Length];
            string[] fromFieldMappings = foreignKeyInfo.FromFieldMappings;
            string[] toFieldMappings = foreignKeyInfo.ToFieldMappings;

            if (fromFieldMappings.Length != toFieldMappings.Length)
            {
                LogManager.GetLogger(typeof (EntityInfoCache)).Fatal(
                    "incorrect relation definition, no of from columns should ne equal to no of to columns");
                throw new IncorrectFieldDefinitionException(
                    "incorrect relation definition, no of from columns should ne equal to no of to columns");
            }

            for (int i = 0; i < fromFieldMappings.Length; i++)
            {
                string fromMapping = fromFieldMappings[i];
                string toMapping = toFieldMappings[i];
                objectMappings[i] = new RelationColumnMapping(fromMapping, toMapping);
            }
            return objectMappings;
        }
    }
}
