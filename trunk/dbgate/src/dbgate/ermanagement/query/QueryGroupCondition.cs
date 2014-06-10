using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition;
using dbgate.ermanagement.query.expr;

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
			var queryGroupCondition = (AbstractSqlQueryGroupCondition) _factory.CreateGroupCondition(QueryGroupConditionExpressionType.RawSql);
			queryGroupCondition.Sql = sql;
			return queryGroupCondition;
        }

        public static IQueryGroupCondition Expression(GroupConditionExpr expr)
        {
            var expressionGroupCondition = (AbstractExpressionGroupCondition) _factory.CreateGroupCondition(QueryGroupConditionExpressionType.Expression);
            expressionGroupCondition.Expr = expr;
            return expressionGroupCondition;
        }
    }
}
