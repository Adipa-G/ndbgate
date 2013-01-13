using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;

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
			AbstractSqlQueryFrom queryFrom = (AbstractSqlQueryFrom) _factory.CreateFrom(QueryFromType.RAW_SQL);
			queryFrom.Sql = sql;
			return queryFrom;
        }
    }
}
