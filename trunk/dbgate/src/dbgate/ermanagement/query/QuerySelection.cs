using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using System;
using dbgate.ermanagement.query.expr;

namespace dbgate.ermanagement.query
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
			AbstractSqlQuerySelection querySelection = (AbstractSqlQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.RawSql);
			querySelection.Sql = sql;
			return querySelection;
        }

		public static IQuerySelection EntityType(Type type)
        {
			AbstractTypeSelection querySelection = (AbstractTypeSelection) _factory.CreateSelection(QuerySelectionExpressionType.EntityType);
			querySelection.EntityType = type;
			return querySelection;
        }
		
		public static IQuerySelection Query(ISelectionQuery query)
		{
			return Query(query,null);
		}
		
		public static IQuerySelection Query(ISelectionQuery query,String alias)
		{
			AbstractSubQuerySelection selection = (AbstractSubQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.Query);
			selection.Query = query;
			if (!string.IsNullOrEmpty(alias))
			{
				selection.Alias = alias;
			}
			return selection;
	 	}
		
		private static IQuerySelection Expression(SelectExpr expr)
		{
            var expressionSelection = (AbstractExpressionSelection) _factory.CreateSelection(QuerySelectionExpressionType.Expression);
	        expressionSelection.Expr = expr;
            return expressionSelection;
		}
		
		public static IQuerySelection Column(Type entityType,String field,String alias)
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
