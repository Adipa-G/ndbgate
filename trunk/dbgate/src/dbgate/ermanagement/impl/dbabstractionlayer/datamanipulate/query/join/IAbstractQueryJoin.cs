using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join
{
	public interface IAbstractQueryJoin : IQueryJoin
	{
		String CreateSql();
	}
}

