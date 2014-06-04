using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractConditionFactory
	{
	    public AbstractConditionFactory()
	    {
	    }

	    public IAbstractCondition CreateCondition (QueryConditionExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryConditionExpressionType.RawSql:
					return new AbstractSqlQueryCondition ();
                case QueryConditionExpressionType.Expression:
                    return new AbstractExpressionCondition();
				default:
					return null;
			}
		}
	}
}

