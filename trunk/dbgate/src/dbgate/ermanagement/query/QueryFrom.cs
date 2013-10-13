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
			AbstractTypeQueryFrom queryFrom = (AbstractTypeQueryFrom) _factory.CreateFrom(QueryFromExpressionType.ENTITY_TYPE);
			queryFrom.EntityType = entityType;
			return queryFrom;
        }
    }
}
