using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group;

namespace dbgate.ermanagement.query
{
    public class QueryGroup
    {
		private static AbstractQueryGroupFactory _factory;

		public static AbstractQueryGroupFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryGroup RawSql(string sql)
        {
			AbstractSqlQueryGroup queryGroup = (AbstractSqlQueryGroup) _factory.CreateGroup(QueryGroupType.RAW_SQL);
			queryGroup.Sql = sql;
			return queryGroup;
        }
    }
}
