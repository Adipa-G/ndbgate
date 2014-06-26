using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@join
{
	public interface IAbstractJoin : IQueryJoin
	{
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

