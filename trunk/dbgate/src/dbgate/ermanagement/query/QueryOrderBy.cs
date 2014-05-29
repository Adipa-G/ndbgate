using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby;

namespace dbgate.ermanagement.query
{
    public class QueryOrderBy
    {
		private static AbstractOrderByFactory _factory;

		public static AbstractOrderByFactory Factory
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
