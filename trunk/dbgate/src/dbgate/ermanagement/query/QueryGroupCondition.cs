using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition;

namespace dbgate.ermanagement.query
{
    public class QueryGroupCondition
    {
		private static AbstractGroupConditionFactory _factory;

		public static AbstractGroupConditionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryGroupCondition RawSql(string sql)
        {
			AbstractSqlQueryGroupCondition queryGroupCondition = (AbstractSqlQueryGroupCondition) _factory.CreateGroupCondition(QueryGroupConditionExpressionType.RAW_SQL);
			queryGroupCondition.Sql = sql;
			return queryGroupCondition;
        }
    }
}
