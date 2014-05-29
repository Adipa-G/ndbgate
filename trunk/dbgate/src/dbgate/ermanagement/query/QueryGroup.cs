using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group;

namespace dbgate.ermanagement.query
{
    public class QueryGroup
    {
		private static AbstractGroupFactory _factory;

		public static AbstractGroupFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryGroup RawSql(string sql)
        {
			AbstractSqlQueryGroup queryGroup = (AbstractSqlQueryGroup) _factory.CreateGroup(QueryGroupExpressionType.RAW_SQL);
			queryGroup.Sql = sql;
			return queryGroup;
        }
    }
}
