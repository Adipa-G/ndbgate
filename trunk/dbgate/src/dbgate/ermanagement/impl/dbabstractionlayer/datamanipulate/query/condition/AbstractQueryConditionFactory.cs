using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractQueryConditionFactory
	{
		public IAbstractQueryCondition CreateCondition (QueryConditionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryConditionExpressionType.RAW_SQL:
					return new AbstractSqlQueryCondition ();
				default:
					return null;
			}
		}
	}
}

