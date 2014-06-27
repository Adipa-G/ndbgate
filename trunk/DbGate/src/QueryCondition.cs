using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
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
