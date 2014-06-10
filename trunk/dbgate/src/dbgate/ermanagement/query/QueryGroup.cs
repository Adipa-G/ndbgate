using System;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group;
using dbgate.ermanagement.query.expr;

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
			var queryGroup = (AbstractSqlQueryGroup) _factory.CreateGroup(QueryGroupExpressionType.RawSql);
			queryGroup.Sql = sql;
			return queryGroup;
        }

        public static IQueryGroup Field(Type entityType, string field)
        {
            var expressionGroup = (AbstractExpressionGroup) _factory.CreateGroup(QueryGroupExpressionType.Expression);
            expressionGroup.Expr = GroupExpr.Build().Field(entityType,field);
            return expressionGroup;
        }
    }
}
