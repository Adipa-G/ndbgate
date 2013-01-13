using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby
{
	public interface IAbstractQueryOrderBy : IQueryOrderBy
	{
		String CreateSql();
	}
}

