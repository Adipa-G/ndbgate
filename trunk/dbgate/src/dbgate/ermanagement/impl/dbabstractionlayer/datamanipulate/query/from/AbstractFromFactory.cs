using System;
using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractFromFactory
	{
		public IAbstractFrom CreateFrom (QueryFromExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryFromExpressionType.RAW_SQL:
					return new AbstractSqlQueryFrom ();
				case QueryFromExpressionType.ENTITY_TYPE:
					return new AbstractTypeFrom ();
				case QueryFromExpressionType.QUERY:
					return new AbstractSubQueryFrom();
				case QueryFromExpressionType.QUERY_UNION:
					return new AbstractUnionFrom();
				default:
					return null;
			}
		}
	}
}

