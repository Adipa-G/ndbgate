using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractGroupFactory
	{
		public IAbstractGroup CreateGroup (QueryGroupExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryGroupExpressionType.RawSql:
					return new AbstractSqlQueryGroup ();
                case QueryGroupExpressionType.Expression:
			        return new AbstractExpressionGroup();
				default:
					return null;
			}
		}
	}
}

