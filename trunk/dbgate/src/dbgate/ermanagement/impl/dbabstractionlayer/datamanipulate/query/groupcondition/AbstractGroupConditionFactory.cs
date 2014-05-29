using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition
{
	public class AbstractGroupConditionFactory
	{
		public IAbstractGroupCondition CreateGroupCondition (QueryGroupConditionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryGroupConditionExpressionType.RAW_SQL:
					return new AbstractSqlQueryGroupCondition ();
				default:
					return null;
			}
		}
	}
}

