using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join
{
	public interface IAbstractJoin : IQueryJoin
	{
		String CreateSql();
	}
}

