using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@from
{
	public interface IAbstractFrom : IQueryFrom
	{
		String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo);
	}
}

