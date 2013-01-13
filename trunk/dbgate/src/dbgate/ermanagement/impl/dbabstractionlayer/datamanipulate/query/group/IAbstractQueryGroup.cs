using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public interface IAbstractQueryGroup : IQueryGroup
	{
		String CreateSql();
	}
}

