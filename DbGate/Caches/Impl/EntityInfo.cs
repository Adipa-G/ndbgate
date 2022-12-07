using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.Exceptions.Common;
using DbGate.Exceptions.Query;

namespace DbGate.Caches.Impl
{
    public class EntityInfo
    {
        private readonly Type entityType;
        private readonly IDictionary<string,PropertyInfo> propertyMap;
        private readonly List<IColumn> columns;
        private readonly List<IRelation> relations;
        private readonly IDictionary<string,string> queries;
        private readonly List<EntityInfo> subEntityInfo;
        private readonly ICollection<EntityRelationColumnInfo> relationColumnInfoList;


        private bool relationColumnsPopulated;
    
        public EntityInfo(Type entityType)
        {
            relationColumnsPopulated = false;
            this.entityType = entityType;
            columns = new List<IColumn>();
            relations = new List<IRelation>();
            propertyMap = new Dictionary<string, PropertyInfo>();
            queries = new Dictionary<string, string>();
            subEntityInfo = new List<EntityInfo>();
            relationColumnInfoList = new List<EntityRelationColumnInfo>();
        }

        public Type EntityType => entityType;

        public ITable TableInfo { get; set; }

        public EntityInfo SuperEntityInfo { get; set; }

        public ICollection<EntityInfo> SubEntityInfo => subEntityInfo.AsReadOnly();

        public void AddSubEntityInfo(EntityInfo subEntityInfo)
        {
            this.subEntityInfo.Add(subEntityInfo);
        }

        public ICollection<IColumn> Columns
        {
            get
            {
                PopulateRelationColumns();
                return columns.AsReadOnly();
            }
        }

        public EntityRelationColumnInfo FindRelationColumnInfo(string attributeName)
        {
            return
                relationColumnInfoList.FirstOrDefault(
                    l => attributeName.Equals(l.Column.AttributeName, StringComparison.InvariantCultureIgnoreCase));
        }
	
	    public IColumn FindColumnByAttribute(string attributeName)
	    {
	        return columns.First(c => attributeName.Equals(c.AttributeName, StringComparison.InvariantCultureIgnoreCase));
	    }

        public ICollection<IRelation> Relations => relations.AsReadOnly();

        public IDictionary<string, string> Queries => new ReadOnlyDictionary<string, string>(queries);

        public ICollection<IColumn> GetKeys()
        {
            var keys = new List<IColumn>();
            foreach (var column in Columns)
            {
                if (column.Key)
                {
                    keys.Add(column);
                }
            }
            return keys;
        }
    
        public void SetFields(ICollection<IField> fields)
        {
            foreach (var field in fields)
            {
                var dbColumn = field as IColumn;
                if (dbColumn != null)
                {
                    columns.Add(dbColumn);
                }
                else
                {
                    var relation = field as IRelation;
                    if (relation != null)
                        relations.Add(relation);
                }
            }
        }

        private void PopulateRelationColumns()
	    {
	        if (relationColumnsPopulated)
	            return;
	

	        foreach (var relation in relations)
	        {
	            var found = HasManualRelationColumnsDefined(relation);
	            if (!found)
	            {
	                CreateRelationColumns(relation);
	            }
	        }

            relationColumnsPopulated = true;
	    }
	
	    private bool HasManualRelationColumnsDefined(IRelation relation)
	    {
	        var found = false;
	        foreach (var mapping in relation.TableColumnMappings)
	        {
	            foreach (var column in columns)
	            {
	                if (column.AttributeName.Equals(mapping.FromField,StringComparison.InvariantCultureIgnoreCase))
	                {
	                    found = true;
	                    break;
	                }
	            }
	            if (found)
	                break;
	        }
	        return found;
	    }
	
	    private void CreateRelationColumns(IRelation relation)
	    {
	        var relationInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);
	        while (relationInfo != null)
	        {
	            var relationKeys = relationInfo.GetKeys();
	            foreach (var relationKey in relationKeys)
	            {
	                RelationColumnMapping matchingMapping = null;
	                foreach (var mapping in relation.TableColumnMappings)
	                {
	                    if (mapping.ToField.Equals(relationKey.AttributeName,StringComparison.InvariantCultureIgnoreCase))
	                    {
	                        matchingMapping = mapping;
	                        break;
	                    }
	                }
	
	                var cloned = relationKey.Clone();
	                cloned.Key =false;
	                if (matchingMapping != null)
	                {
	                    cloned.AttributeName = matchingMapping.FromField;
	                    cloned.ColumnName = AbstractColumn.PredictColumnName(matchingMapping.FromField);
	                }
	                cloned.Nullable = relation.Nullable;

                    columns.Add(cloned);
	                relationColumnInfoList.Add(new EntityRelationColumnInfo(cloned,relation,matchingMapping));
	            }
	            relationInfo = relationInfo.SuperEntityInfo;
	        }
	    }
    
        public string GetLoadQuery(IDbLayer dbLayer)
        {
            const string queryId = "LOAD";
            var query = GetQuery(queryId);
            if (query == null)
            {
                PopulateRelationColumns();
                query = dbLayer.DataManipulate().CreateLoadQuery(TableInfo.TableName,Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Load Query building failed for table {0} class {1}",
                                                               TableInfo, EntityType.FullName));
            }
            return query;
        }
    
        public string GetInsertQuery(IDbLayer dbLayer)
        {
            const string queryId = "INSERT";
            var query = GetQuery(queryId);
            if (query == null)
            {
                PopulateRelationColumns();
                query = dbLayer.DataManipulate().CreateInsertQuery(TableInfo.TableName,Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Insert Query building failed for table {0} class {1}",
                                                               TableInfo, EntityType.FullName));
            }
            return query;
        }
    
        public string GetUpdateQuery(IDbLayer dbLayer)
        {
            const string queryId = "UPDATE";
            var query = GetQuery(queryId);
            if (query == null)
            {
                PopulateRelationColumns();
                query = dbLayer.DataManipulate().CreateUpdateQuery(TableInfo.TableName, Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Update Query building failed for table {0} class {1}",
                                                               TableInfo, EntityType.FullName));
            }
            return query;
        }
    
        public string GetDeleteQuery(IDbLayer dbLayer)
        {
            const string queryId = "DELETE";
            var query = GetQuery(queryId);
            if (query == null)
            {
                PopulateRelationColumns();
                query = dbLayer.DataManipulate().CreateDeleteQuery(TableInfo.TableName, Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Delete Query building failed for table {0} class {1}",TableInfo,EntityType.FullName));
            }
            return query;
        }
    
        public string GetRelationObjectLoad(IDbLayer dbLayer, IRelation relation)
        {
            var queryId = relation.RelationShipName + "_" + relation.RelatedObjectType.FullName;
            var query = GetQuery(queryId);
            if (query == null)
            {
                query = dbLayer.DataManipulate().CreateRelatedObjectsLoadQuery(relation);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Child loading Query building failed for table {0} class {1} child object type {2}",TableInfo,EntityType.FullName,relation.RelatedObjectType.FullName));
            }
            return query;
        }
    
        private string GetQuery(string id)
        {
            if (Queries.ContainsKey(id))
                return Queries[id];
            return null;
        }
    
        private void SetQuery(String id,String query)
        {
            lock (Queries)
            {
                queries[id] = query;
            }
        }

        public PropertyInfo GetProperty(IColumn column)
        {
            return GetProperty(column.AttributeName);
        }

        public PropertyInfo GetProperty(string propertyName)
        {
            var cacheKey = "prop_" + propertyName;

            if (propertyMap.ContainsKey(cacheKey))
            {
                return propertyMap[cacheKey];
            }

            lock (propertyMap)
            {
                if (propertyMap.ContainsKey(cacheKey))
                {
                    return propertyMap[cacheKey];
                }

                try
                {
                    var propertyInfo = EntityType.GetProperty(propertyName);
                    if (propertyInfo == null)
                        throw CreatePropertyNotFoundException(propertyName);

                    propertyMap.Add(cacheKey, propertyInfo);
                    return propertyInfo;
                }
                catch (Exception)
                {
                    throw CreatePropertyNotFoundException(propertyName);
                }
            }
        }

        private Exception CreatePropertyNotFoundException(string propertyName)
        {
            return new PropertyNotFoundException(string.Format("unable to find property {0} of type {1}",
                propertyName, EntityType.FullName));
        }
    }
}
