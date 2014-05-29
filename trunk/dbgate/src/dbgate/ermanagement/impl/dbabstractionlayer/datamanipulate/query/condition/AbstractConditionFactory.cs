using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractConditionFactory
	{
		public IAbstractCondition CreateCondition (QueryConditionExpressionType expressionType)
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

