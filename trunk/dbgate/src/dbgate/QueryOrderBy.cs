using System;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@orderby;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate
{
    public class QueryOrderBy
    {
		private static AbstractOrderByFactory _factory;

		public static AbstractOrderByFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryOrderBy RawSql(string sql)
        {
			var queryOrderBy = (AbstractSqlQueryOrderBy) _factory.CreateOrderBy(QueryOrderByExpressionType.RawSql);
			queryOrderBy.Sql = sql;
			return queryOrderBy;
        }

        public static IQueryOrderBy Field(Type type,string field)
        {
            return Field(type,field,QueryOrderType.Ascend);
        }

        public static IQueryOrderBy Field(Type type,string field,QueryOrderType orderType)
        {
            var expressionOrderBy = (AbstractExpressionOrderBy) _factory.CreateOrderBy(QueryOrderByExpressionType.Expression);
            expressionOrderBy.Expr = OrderByExpr.Build().Field(type,field);
            expressionOrderBy.OrderType = orderType;
            return expressionOrderBy;
        }
    }
}
