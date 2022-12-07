using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryCondition
    {
		private static AbstractConditionFactory factory;

		public static AbstractConditionFactory Factory
		{
			set => factory = value;
        }

        public static IQueryCondition RawSql(string sql)
        {
			var queryCondition = (AbstractSqlQueryCondition) factory.CreateCondition(QueryConditionExpressionType.RawSql);
			queryCondition.Sql = sql;
			return queryCondition;
        }

        public static IQueryCondition Expression(ConditionExpr expr)
        {
            var queryCondition = (AbstractExpressionCondition)factory.CreateCondition(QueryConditionExpressionType.Expression);
            queryCondition.Expr = expr;
            return queryCondition;
        }
    }
}
