using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractQueryGroupFactory
	{
		public IAbstractQueryGroup CreateGroup (QueryGroupExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryGroupExpressionType.RAW_SQL:
					return new AbstractSqlQueryGroup ();
				default:
					return null;
			}
		}
	}
}

