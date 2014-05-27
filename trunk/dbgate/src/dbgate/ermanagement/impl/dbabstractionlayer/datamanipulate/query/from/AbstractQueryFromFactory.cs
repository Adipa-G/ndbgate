using System;
using dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
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
				case QueryFromExpressionType.QUERY:
					return new AbstractQueryQueryFrom();
				case QueryFromExpressionType.QUERY_UNION:
					return new AbstractQueryUnionQueryFrom();
				default:
					return null;
			}
		}
	}
}

