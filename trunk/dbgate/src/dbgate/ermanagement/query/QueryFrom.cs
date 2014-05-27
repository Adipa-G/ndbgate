using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using System;

namespace dbgate.ermanagement.query
{
    public class QueryFrom
    {
        private static AbstractQueryFromFactory _factory;

		public static AbstractQueryFromFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryFrom RawSql(string sql)
        {
			AbstractSqlQueryFrom queryFrom = (AbstractSqlQueryFrom) _factory.CreateFrom(QueryFromExpressionType.RAW_SQL);
			queryFrom.Sql = sql;
			return queryFrom;
        }

		public static IQueryFrom EntityType(Type entityType)
        {
			var queryFrom = (AbstractTypeQueryFrom) _factory.CreateFrom(QueryFromExpressionType.ENTITY_TYPE);
			queryFrom.EntityType = entityType;
			return queryFrom;
        }
		
		public static IQueryFrom EntityType(Type entityType,String alias)
		{
			var queryFrom = (AbstractTypeQueryFrom) _factory.CreateFrom(QueryFromExpressionType.ENTITY_TYPE);
			queryFrom.EntityType = entityType;
			if (!string.IsNullOrEmpty(alias))
		 	{
		 		queryFrom.Alias = alias;
		 	}
		 	return queryFrom;
		}
		 	
	 	public static IQueryFrom Query(ISelectionQuery query)
	 	{
	 		return Query(query, null);
	 	}
		 	
	 	public static IQueryFrom Query(ISelectionQuery query,String alias)
	 	{
	 		var queryFrom = (AbstractQueryQueryFrom) _factory.CreateFrom(QueryFromExpressionType.QUERY);
	 		queryFrom.Query = query;;
	 		if (!string.IsNullOrEmpty(alias))
	 		{
	 			queryFrom.Alias = alias;
	 		}
	 		return queryFrom;
	 	}
		 	
	 	public static IQueryFrom QueryUnion(bool all,ISelectionQuery[] queries)
	 	{
	 		var queryFrom = (AbstractQueryUnionQueryFrom) _factory.CreateFrom(QueryFromExpressionType.QUERY_UNION);
	 		queryFrom.Queries = queries;
	 		queryFrom.All = all;
			return queryFrom;  
	 	}
    }
}
