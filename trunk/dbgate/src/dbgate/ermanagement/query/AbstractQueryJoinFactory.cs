using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join
{
	public class AbstractQueryJoinFactory
	{
		public IAbstractQueryJoin CreateJoin (QueryJoinExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryJoinExpressionType.RAW_SQL:
					return new AbstractSqlQueryJoin ();
				default:
					return null;
			}
		}
	}
}

