using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractQuerySelectionFactory
	{
		public IAbstractQuerySelection CreateSelection (QuerySelectionType selectionType)
		{
			switch (selectionType) 
			{
				case QuerySelectionType.RAW_SQL:
					return new AbstractSqlQuerySelection ();
				default:
					return null;
			}
		}
	}
}

