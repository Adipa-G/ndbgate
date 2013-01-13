using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractQuerySelectionFactory
	{
		public IAbstractQuerySelection CreateSelection (QuerySelectionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QuerySelectionExpressionType.RAW_SQL:
					return new AbstractSqlQuerySelection ();
				default:
					return null;
			}
		}
	}
}

