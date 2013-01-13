using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition;

namespace dbgate.ermanagement.query
{
    public class QueryCondition
    {
		private static AbstractQueryConditionFactory _factory;

		public static AbstractQueryConditionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryCondition RawSql(string sql)
        {
			AbstractSqlQueryCondition queryCondition = (AbstractSqlQueryCondition) _factory.CreateCondition(QueryConditionType.RAW_SQL);
			queryCondition.Sql = sql;
			return queryCondition;
        }
    }
}
