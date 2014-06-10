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
				case QueryGroupConditionExpressionType.RawSql:
					return new AbstractSqlQueryGroupCondition ();
                case QueryGroupConditionExpressionType.Expression:
                    return new AbstractExpressionGroupCondition();
				default:
					return null;
			}
		}
	}
}

