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
        private readonly Type _entityType;
        private readonly IDictionary<string,PropertyInfo> _propertyMap;
        private readonly List<IColumn> _columns;
        private readonly List<IRelation> _relations;
        private readonly IDictionary<string,string> _queries;
        private readonly List<EntityInfo> _subEntityInfo;
        private readonly ICollection<EntityRelationColumnInfo> _relationColumnInfoList;


        private bool _relationColumnsPopulated;
    
        public EntityInfo(Type entityType)
        {
            _relationColumnsPopulated = false;
            _entityType = entityType;
            _columns = new List<IColumn>();
            _relations = new List<IRelation>();
            _propertyMap = new Dictionary<string, PropertyInfo>();
            _queries = new Dictionary<string, string>();
            _subEntityInfo = new List<EntityInfo>();
            _relationColumnInfoList = new List<EntityRelationColumnInfo>();
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        public ITable TableInfo { get; set; }

        public EntityInfo SuperEntityInfo { get; set; }

        public ICollection<EntityInfo> SubEntityInfo
	    {
            get { return  _subEntityInfo.AsReadOnly(); }
	    }

        public void AddSubEntityInfo(EntityInfo subEntityInfo)
        {
            _subEntityInfo.Add(subEntityInfo);
        }

        public ICollection<IColumn> Columns
        {
            get
            {
                PopulateRelationColumns();
                return _columns.AsReadOnly();
            }
        }

        public EntityRelationColumnInfo FindRelationColumnInfo(string attributeName)
        {
            return
                _relationColumnInfoList.FirstOrDefault(
                    l => attributeName.Equals(l.Column.AttributeName, StringComparison.InvariantCultureIgnoreCase));
        }
	
	    public IColumn FindColumnByAttribute(string attributeName)
	    {
	        return _columns.First(c => attributeName.Equals(c.AttributeName, StringComparison.InvariantCultureIgnoreCase));
	    }

        public ICollection<IRelation> Relations
        {
            get { return _relations.AsReadOnly(); }
        }

        public IDictionary<string, string> Queries
        {
            get { return new ReadOnlyDictionary<string, string>(_queries); }
        }

        public ICollection<IColumn> GetKeys()
        {
            var keys = new List<IColumn>();
            foreach (IColumn column in Columns)
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
            foreach (IField field in fields)
            {
                var dbColumn = field as IColumn;
                if (dbColumn != null)
                {
                    _columns.Add(dbColumn);
                }
                else
                {
                    var relation = field as IRelation;
                    if (relation != null)
                        _relations.Add(relation);
                }
            }
        }

        private void PopulateRelationColumns()
	    {
	        if (_relationColumnsPopulated)
	            return;
	

	        foreach (IRelation relation in _relations)
	        {
	            bool found = HasManualRelationColumnsDefined(relation);
	            if (!found)
	            {
	                CreateRelationColumns(relation);
	            }
	        }

            _relationColumnsPopulated = true;
	    }
	
	    private bool HasManualRelationColumnsDefined(IRelation relation)
	    {
	        bool found = false;
	        foreach (RelationColumnMapping mapping in relation.TableColumnMappings)
	        {
	            foreach (IColumn column in _columns)
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
	        EntityInfo relationInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);
	        while (relationInfo != null)
	        {
	            ICollection<IColumn> relationKeys = relationInfo.GetKeys();
	            foreach (IColumn relationKey in relationKeys)
	            {
	                RelationColumnMapping matchingMapping = null;
	                foreach (RelationColumnMapping mapping in relation.TableColumnMappings)
	                {
	                    if (mapping.ToField.Equals(relationKey.AttributeName,StringComparison.InvariantCultureIgnoreCase))
	                    {
	                        matchingMapping = mapping;
	                        break;
	                    }
	                }
	
	                IColumn cloned = relationKey.Clone();
	                cloned.Key =false;
	                if (matchingMapping != null)
	                {
	                    cloned.AttributeName = matchingMapping.FromField;
	                    cloned.ColumnName = AbstractColumn.PredictColumnName(matchingMapping.FromField);
	                }
	                cloned.Nullable = relation.Nullable;

                    _columns.Add(cloned);
	                _relationColumnInfoList.Add(new EntityRelationColumnInfo(cloned,relation,matchingMapping));
	            }
	            relationInfo = relationInfo.SuperEntityInfo;
	        }
	    }
    
        public string GetLoadQuery(IDbLayer dbLayer)
        {
            const string queryId = "LOAD";
            string query = GetQuery(queryId);
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
            string query = GetQuery(queryId);
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
            string query = GetQuery(queryId);
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
            string query = GetQuery(queryId);
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
            string queryId = relation.RelationShipName + "_" + relation.RelatedObjectType.FullName;
            string query = GetQuery(queryId);
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
                _queries[id] = query;
            }
        }

        public PropertyInfo GetProperty(IColumn column)
        {
            return GetProperty(column.AttributeName);
        }

        public PropertyInfo GetProperty(string propertyName)
        {
            string cacheKey = "prop_" + propertyName;
            if (_propertyMap.ContainsKey(cacheKey))
            {
                return _propertyMap[cacheKey];
            }

            try
            {
                PropertyInfo propertyInfo = EntityType.GetProperty(propertyName);
                if (propertyInfo == null)
                    throw CreatePropertyNotFoundException(propertyName);

                lock (_propertyMap)
                {
                    _propertyMap.Add(cacheKey, propertyInfo);
                }
                return propertyInfo;
            }
            catch (Exception)
            {
                throw CreatePropertyNotFoundException(propertyName);
            }
        }

        private Exception CreatePropertyNotFoundException(string propertyName)
        {
            return new PropertyNotFoundException(string.Format("unable to find property {0} of type {1}",
                propertyName, EntityType.FullName));
        }
    }
}
