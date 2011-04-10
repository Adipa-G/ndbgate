using System;
using System.Collections.Generic;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.caches.impl
{
    public class FieldCache : IFieldCache
    {
        private static readonly Dictionary<string, ICollection<IField>> ColumnCache = new Dictionary<string, ICollection<IField>>();

        public ICollection<IField> GetFields(Type type)
        {
            ICollection<IField> retFields;
            String cacheKey = GetCacheKey(type);

            if (ColumnCache.ContainsKey(cacheKey))
            {
                retFields = ColumnCache[cacheKey];
            }
            else
            {
                throw new FieldCacheMissException(String.Format("No cache entry found for {0}", type.FullName));  
            }
            return retFields;
        }

        public ICollection<IDbColumn> GetDbColumns(Type type)
        {
            return GetDbColumns(GetFields(type),false);
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

        public ICollection<IDbRelation> GetDbRelations(Type type)
        {
            return GetDbRelations(GetFields(type));
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

        public void Register(Type type, ICollection<IField> fields)
        {
            String cacheKey = GetCacheKey(type);
            lock (ColumnCache)
            {
                ColumnCache.Add(cacheKey, fields);
            }
        }

        public void Register(Type type,IServerRoDbClass roDbClass) 
        {
            String cacheKey = GetCacheKey(type);
            if (ColumnCache.ContainsKey(cacheKey))
            {
                return;
            }
            ICollection<IField> applicableDbColumns = DbClassAttributeExtractionUtils.GetAllFields(roDbClass,type);
            if (applicableDbColumns == null)
            {
                throw new NoFieldsFoundException(String.Format("Can't find list of fields for type {0}",type.FullName));
            }
            lock (ColumnCache)
            {
                ColumnCache.Add(cacheKey, applicableDbColumns); 
            } 
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