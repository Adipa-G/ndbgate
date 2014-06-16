using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dbgate.ermanagement.exceptions.common;
using dbgate.ermanagement.exceptions.query;
using dbgate.ermanagement.impl.dbabstractionlayer;

namespace dbgate.ermanagement.caches.impl
{
    public class EntityInfo
    {
        private readonly Type _entityType;
        private readonly IDictionary<string,PropertyInfo> _propertyMap;
        private readonly ICollection<IColumn> _columns;
        private readonly ICollection<IRelation> _relations;
        private readonly IDictionary<string,string> _queries;
    
        public EntityInfo(Type entityType)
        {
            _entityType = entityType;
            _columns = new List<IColumn>();
            _relations = new List<IRelation>();
            _propertyMap = new Dictionary<string, PropertyInfo>();
            _queries = new Dictionary<string, string>();
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        public string TableName { get; set; }

        public EntityInfo SuperEntityInfo { get; set; }

        public ICollection<IColumn> Columns
        {
            get { return _columns; }
        }

        public ICollection<IRelation> Relations
        {
            get { return _relations; }
        }

        public IDictionary<string, string> Queries
        {
            get { return _queries; }
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
    
        public string GetLoadQuery(IDbLayer dbLayer)
        {
            const string queryId = "LOAD";
            string query = GetQuery(queryId);
            if (query == null)
            {
                query = dbLayer.DataManipulate().CreateLoadQuery(TableName,Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Load Query building failed for table {0} class {1}",
                                                               TableName, EntityType.FullName));
            }
            return query;
        }
    
        public string GetInsertQuery(IDbLayer dbLayer)
        {
            const string queryId = "INSERT";
            string query = GetQuery(queryId);
            if (query == null)
            {
                query = dbLayer.DataManipulate().CreateInsertQuery(TableName,Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Insert Query building failed for table {0} class {1}",
                                                               TableName, EntityType.FullName));
            }
            return query;
        }
    
        public string GetUpdateQuery(IDbLayer dbLayer)
        {
            const string queryId = "UPDATE";
            string query = GetQuery(queryId);
            if (query == null)
            {
                query = dbLayer.DataManipulate().CreateUpdateQuery(TableName, Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Update Query building failed for table {0} class {1}",
                                                               TableName, EntityType.FullName));
            }
            return query;
        }
    
        public string GetDeleteQuery(IDbLayer dbLayer)
        {
            const string queryId = "DELETE";
            string query = GetQuery(queryId);
            if (query == null)
            {
                query = dbLayer.DataManipulate().CreateDeleteQuery(TableName, Columns);
                SetQuery(queryId,query);
            }
            if (query == null)
            {
                throw new QueryBuildingException(String.Format("Delete Query building failed for table {0} class {1}",TableName,EntityType.FullName));
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
                throw new QueryBuildingException(String.Format("Child loading Query building failed for table {0} class {1} child object type {2}",TableName,EntityType.FullName,relation.RelatedObjectType.FullName));
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
                Queries[id] = query;
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
                lock (_propertyMap)
                {
                    _propertyMap.Add(cacheKey, propertyInfo);
                }
                return propertyInfo;
            }
            catch (Exception ex)
            {
                throw new PropertyNotFoundException(string.Format("unable to find property {0} of type {1}",
                                                                  propertyName, EntityType.FullName));
            }
           
        }
    }
}
