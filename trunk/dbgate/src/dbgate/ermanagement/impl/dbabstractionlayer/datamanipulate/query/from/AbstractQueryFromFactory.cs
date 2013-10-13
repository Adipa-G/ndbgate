using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractQueryFromFactory
	{
		public IAbstractQueryFrom CreateFrom (QueryFromExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryFromExpressionType.RAW_SQL:
					return new AbstractSqlQueryFrom ();
				case QueryFromExpressionType.ENTITY_TYPE:
					return new AbstractTypeQueryFrom ();
				default:
					return null;
			}
		}
	}
}

