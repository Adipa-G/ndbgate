using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public interface IAbstractQueryFrom : IQueryFrom
	{
		String CreateSql();
	}
}

