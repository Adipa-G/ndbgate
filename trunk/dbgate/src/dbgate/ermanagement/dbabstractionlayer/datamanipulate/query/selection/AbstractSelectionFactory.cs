using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractSelectionFactory
	{
		public IAbstractSelection CreateSelection (QuerySelectionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QuerySelectionExpressionType.RawSql:
					return new AbstractSqlQuerySelection ();
				case QuerySelectionExpressionType.EntityType:
					return new AbstractTypeSelection();
				case QuerySelectionExpressionType.Expression:
			        return new AbstractExpressionSelection();
				default:
					return null;
			}
		}
	}
}

