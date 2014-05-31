using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using System;

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
			AbstractSqlQuerySelection querySelection = (AbstractSqlQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.RAW_SQL);
			querySelection.Sql = sql;
			return querySelection;
        }

		public static IQuerySelection EntityType(Type type)
        {
			AbstractTypeSelection querySelection = (AbstractTypeSelection) _factory.CreateSelection(QuerySelectionExpressionType.ENTITY_TYPE);
			querySelection.EntityType = type;
			return querySelection;
        }
		
		public static IQuerySelection Query(ISelectionQuery query)
		{
			return Query(query,null);
		}
		
		public static IQuerySelection Query(ISelectionQuery query,String alias)
		{
			AbstractSubQuerySelection selection = (AbstractSubQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.QUERY);
			selection.Query = query;
			if (!string.IsNullOrEmpty(alias))
			{
				selection.Alias = alias;
			}
			return selection;
	 	}
		
		private static IQuerySelection ColumnOperation(QuerySelectionExpressionType expressionType, Type entityType,String field,String alias)
		{
			BaseColumnOperation selection = (BaseColumnOperation) _factory.CreateSelection(expressionType);
			selection.EntityType = entityType;
			selection.Field = field;
			if (!string.IsNullOrEmpty(alias))
			{
				selection.Alias = alias;
			}
			return selection;
		}
		
		public static IQuerySelection Column(Type entityType,String field,String alias)
		{
			return ColumnOperation(QuerySelectionExpressionType.COLUMN,entityType,field,alias);
		}
		
		public static IQuerySelection Sum(Type entityType,String field,String alias)
		{
			return ColumnOperation(QuerySelectionExpressionType.SUM,entityType,field,alias);
		}
		
		public static IQuerySelection Count(Type entityType,String field,String alias)
		{
			return ColumnOperation(QuerySelectionExpressionType.COUNT,entityType,field,alias);
		}
		
		public static IQuerySelection CustFunction(String sqlFunction,Type entityType,String field,String alias)
		{
			AbstractCustFuncSelection selection = (AbstractCustFuncSelection)ColumnOperation(QuerySelectionExpressionType.CUST_FUNC,entityType,field,alias);
			selection.Function = sqlFunction;
			return selection;
		}
    }
}
