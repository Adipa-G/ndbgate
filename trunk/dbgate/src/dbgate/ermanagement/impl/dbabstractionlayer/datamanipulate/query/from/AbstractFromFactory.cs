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
				case QueryFromExpressionType.RawSql:
					return new AbstractSqlQueryFrom ();
				case QueryFromExpressionType.EntityType:
					return new AbstractTypeFrom ();
				case QueryFromExpressionType.Query:
					return new AbstractSubQueryFrom();
				case QueryFromExpressionType.QueryUnion:
					return new AbstractUnionFrom();
				default:
					return null;
			}
		}
	}
}

