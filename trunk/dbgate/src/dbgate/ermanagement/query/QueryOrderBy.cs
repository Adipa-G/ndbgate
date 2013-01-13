using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby;

namespace dbgate.ermanagement.query
{
    public class QueryOrderBy
    {
		private static AbstractQueryOrderByFactory _factory;

		public static AbstractQueryOrderByFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryOrderBy RawSql(string sql)
        {
			var queryOrderBy = (AbstractSqlQueryOrderBy) _factory.CreateOrderBy(QueryOrderByExpressionType.RAW_SQL);
			queryOrderBy.Sql = sql;
			return queryOrderBy;
        }
    }
}
