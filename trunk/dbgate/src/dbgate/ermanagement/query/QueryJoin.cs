using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join;

namespace dbgate.ermanagement.query
{
    public class QueryJoin
    {
		private static AbstractQueryJoinFactory _factory;

		public static AbstractQueryJoinFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryJoin RawSql(string sql)
        {
			var queryJoin = (AbstractSqlQueryJoin) _factory.CreateJoin(QueryJoinExpressionType.RAW_SQL);
			queryJoin.Sql = sql;
			return queryJoin;
        }
    }
}
