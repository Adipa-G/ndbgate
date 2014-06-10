using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join
{
	public class AbstractJoinFactory
	{
		public IAbstractJoin CreateJoin (QueryJoinExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryJoinExpressionType.RawSql:
					return new AbstractSqlQueryJoin ();
                case QueryJoinExpressionType.Type:
			        return new AbstractTypeJoin();
				default:
					return null;
			}
		}
	}
}

