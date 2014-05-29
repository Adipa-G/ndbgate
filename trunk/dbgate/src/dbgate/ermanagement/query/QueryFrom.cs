using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using System;

namespace dbgate.ermanagement.query
{
    public class QueryFrom
    {
        private static AbstractFromFactory _factory;

		public static AbstractFromFactory Factory
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
			var typeFrom = (AbstractTypeFrom) _factory.CreateFrom(QueryFromExpressionType.ENTITY_TYPE);
			typeFrom.EntityType = entityType;
			return typeFrom;
        }
		
		public static IQueryFrom EntityType(Type entityType,String alias)
		{
			var typeFrom = (AbstractTypeFrom) _factory.CreateFrom(QueryFromExpressionType.ENTITY_TYPE);
			typeFrom.EntityType = entityType;
			if (!string.IsNullOrEmpty(alias))
		 	{
		 		typeFrom.Alias = alias;
		 	}
		 	return typeFrom;
		}
		 	
	 	public static IQueryFrom Query(ISelectionQuery query)
	 	{
	 		return Query(query, null);
	 	}
		 	
	 	public static IQueryFrom Query(ISelectionQuery query,String alias)
	 	{
	 		var queryFromSub = (AbstractSubQueryFrom) _factory.CreateFrom(QueryFromExpressionType.QUERY);
	 		queryFromSub.Query = query;;
	 		if (!string.IsNullOrEmpty(alias))
	 		{
	 			queryFromSub.Alias = alias;
	 		}
	 		return queryFromSub;
	 	}
		 	
	 	public static IQueryFrom QueryUnion(bool all,ISelectionQuery[] queries)
	 	{
	 		var queryFrom = (AbstractUnionFrom) _factory.CreateFrom(QueryFromExpressionType.QUERY_UNION);
	 		queryFrom.Queries = queries;
	 		queryFrom.All = all;
			return queryFrom;  
	 	}
    }
}
