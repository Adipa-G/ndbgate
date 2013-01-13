using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractQueryConditionFactory
	{
		public IAbstractQueryCondition CreateCondition (QueryConditionType conditionType)
		{
			switch (conditionType) 
			{
				case QueryConditionType.RAW_SQL:
					return new AbstractSqlQueryCondition ();
				default:
					return null;
			}
		}
	}
}

