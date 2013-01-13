using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition
{
	public interface IAbstractQueryGroupCondition : IQueryGroupCondition
	{
		String CreateSql();
	}
}

