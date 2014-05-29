using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public interface IAbstractFrom : IQueryFrom
	{
		String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo);
	}
}

