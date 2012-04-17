using System;
using System.Collections.Generic;
using System.Reflection;
using dbgate.ermanagement.exceptions;
using log4net;

namespace dbgate.ermanagement.impl.utils
{
    public class DbClassAttributeExtractionUtils
    {
        public static string GetTableName(IServerRoDbClass serverRoDbClass, Type type)
        {
            string tableName = null;
            if (serverRoDbClass is IManagedDbClass)
            {
                Dictionary<Type, string> tableNames = ((IManagedDbClass) serverRoDbClass).TableNames;
                if (tableNames != null
                    && tableNames.ContainsKey(type))
                {
                    tableName = tableNames[type];
                }
            }
            if (tableName == null)
            {
                object[] annotations = type.GetCustomAttributes(false);
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

        public static ICollection<IField> GetAllFields(IServerRoDbClass serverRoDbClass, Type type)
        {
            Dictionary<Type, ICollection<IField>> fieldInfoMap = null;
            if (serverRoDbClass is IManagedRoDbClass)
            {
                fieldInfoMap = ((IManagedRoDbClass) serverRoDbClass).FieldInfo;
            }
            var fields = new List<IField>();
            Type[] superTypes = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(type,
                                                                                       new[] {typeof (IServerRoDbClass)});

            for (int i = 0; i < superTypes.Length; i++)
            {
                Type superType = superTypes[i];
                fields.AddRange(GetAllFields(fieldInfoMap, superType, i > 0));
            }

            return fields;
        }

        private static ICollection<IField> GetAllFields(Dictionary<Type, ICollection<IField>> fieldInfoMap, Type type,
                                                        bool superClass)
        {
            var fields = new List<IField>();

            ICollection<IField> infoFields = fieldInfoMap != null && fieldInfoMap.ContainsKey(type)? fieldInfoMap[type] : null;
            if (infoFields != null)
            {
                foreach (IField infoField in infoFields)
                {
                    if (superClass
                        && infoField is IDbColumn
                        && ((IDbColumn) infoField).SubClassCommonColumn)
                    {
                        fields.Add(infoField);
                    }
                    else if (!superClass)
                    {
                        fields.Add(infoField);
                    }
                }
            }

            PropertyInfo[] dbClassFields = type.GetProperties();
            foreach (PropertyInfo propertyInfo in dbClassFields)
            {
                if (propertyInfo.DeclaringType != type)
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
            string [] fromColumnMappings = foreignKeyInfo.FromColumnMappings;
            string [] toColumnMappings = foreignKeyInfo.ToColumnMappings;

            if (fromColumnMappings.Length != toColumnMappings.Length)
            {
                LogManager.GetLogger(typeof(DbClassAttributeExtractionUtils)).Fatal("incorrect relation definition, no of from columns should ne equal to no of to columns");
                throw new IncorrectFieldDefinitionException("incorrect relation definition, no of from columns should ne equal to no of to columns");
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
                                                         ,foreignKeyInfo.ReverseRelation
                                                         ,foreignKeyInfo.NonIdentifyingRelation,foreignKeyInfo.Lazy);

            return relation;
        }
    }
}
