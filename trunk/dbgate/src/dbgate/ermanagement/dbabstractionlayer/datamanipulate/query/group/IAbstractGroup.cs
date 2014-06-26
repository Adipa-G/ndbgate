using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@group
{
	public interface IAbstractGroup : IQueryGroup
	{
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

