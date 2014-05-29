using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractSelectionFactory
	{
		public IAbstractSelection CreateSelection (QuerySelectionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QuerySelectionExpressionType.RAW_SQL:
					return new AbstractSqlQuerySelection ();
				case QuerySelectionExpressionType.ENTITY_TYPE:
					return new AbstractTypeSelection();
				case QuerySelectionExpressionType.QUERY:
					return new AbstractSubQuerySelection();
				default:
					return null;
			}
		}
	}
}

