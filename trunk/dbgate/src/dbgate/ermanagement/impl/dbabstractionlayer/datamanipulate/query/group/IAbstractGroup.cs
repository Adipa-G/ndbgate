using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public interface IAbstractGroup : IQueryGroup
	{
		String CreateSql();
	}
}

