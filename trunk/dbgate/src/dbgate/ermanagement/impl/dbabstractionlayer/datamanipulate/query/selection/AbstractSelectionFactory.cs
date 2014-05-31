using System;
using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
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
				case QuerySelectionExpressionType.COLUMN:
					return new AbstractColumnSelection();
				case QuerySelectionExpressionType.COUNT:
					return new AbstractCountSelection();
				case QuerySelectionExpressionType.SUM:
					return new AbstractSumSelection();
				case QuerySelectionExpressionType.CUST_FUNC:
					return new AbstractCustFuncSelection();
				default:
					return null;
			}
		}
	}
}

