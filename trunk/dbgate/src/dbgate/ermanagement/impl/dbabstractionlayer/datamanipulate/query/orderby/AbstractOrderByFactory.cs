using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby
{
	public class AbstractOrderByFactory
	{
		public IAbstractOrderBy CreateOrderBy (QueryOrderByExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryOrderByExpressionType.RAW_SQL:
					return new AbstractSqlQueryOrderBy ();
				default:
					return null;
			}
		}
	}
}

