using System;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.selection;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate
{
    public class QuerySelection
    {
		private static AbstractSelectionFactory _factory;

		public static AbstractSelectionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQuerySelection RawSql(string sql)
        {
			var querySelection = (AbstractSqlQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.RawSql);
			querySelection.Sql = sql;
			return querySelection;
        }

		public static IQuerySelection EntityType(Type type)
        {
			var querySelection = (AbstractTypeSelection) _factory.CreateSelection(QuerySelectionExpressionType.EntityType);
			querySelection.EntityType = type;
			return querySelection;
        }
		
		public static IQuerySelection Query(ISelectionQuery query,String alias)
		{
            var expressionSelection = (AbstractExpressionSelection) _factory.CreateSelection(QuerySelectionExpressionType.Expression);
            expressionSelection.Expr = SelectExpr.Build().Query(query,alias);
            return expressionSelection;
	 	}
		
		private static IQuerySelection Expression(SelectExpr expr)
		{
            var expressionSelection = (AbstractExpressionSelection) _factory.CreateSelection(QuerySelectionExpressionType.Expression);
	        expressionSelection.Expr = expr;
            return expressionSelection;
		}

        public static IQuerySelection Field(String field)
        {
            return Expression(SelectExpr.Build().Field(field));
        }

        public static IQuerySelection Field(String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(field, alias));
        }
		
		public static IQuerySelection Field(Type entityType,String field,String alias)
		{
		    return Expression(SelectExpr.Build().Field(entityType, field, alias));
		}
		
		public static IQuerySelection Sum(Type entityType,String field,String alias)
		{
		    return Expression(SelectExpr.Build().Field(entityType, field, alias).Sum());
		}
		
		public static IQuerySelection Count(Type entityType,String field,String alias)
		{
            return Expression(SelectExpr.Build().Field(entityType, field, alias).Count());
		}
		
		public static IQuerySelection CustFunction(String sqlFunction,Type entityType,String field,String alias)
		{
            return Expression(SelectExpr.Build().Field(entityType, field, alias).CustFunc(sqlFunction));
		}
    }
}
