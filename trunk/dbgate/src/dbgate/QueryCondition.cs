using dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.condition;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate
{
    public class QueryCondition
    {
		private static AbstractConditionFactory _factory;

		public static AbstractConditionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryCondition RawSql(string sql)
        {
			var queryCondition = (AbstractSqlQueryCondition) _factory.CreateCondition(QueryConditionExpressionType.RawSql);
			queryCondition.Sql = sql;
			return queryCondition;
        }

        public static IQueryCondition Expression(ConditionExpr expr)
        {
            var queryCondition = (AbstractExpressionCondition)_factory.CreateCondition(QueryConditionExpressionType.Expression);
            queryCondition.Expr = expr;
            return queryCondition;
        }
    }
}
