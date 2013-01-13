using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public interface IAbstractQueryCondition : IQueryCondition
	{
		String CreateSql();
	}
}

