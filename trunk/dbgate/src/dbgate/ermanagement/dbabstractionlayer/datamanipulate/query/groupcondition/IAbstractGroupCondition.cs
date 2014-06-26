using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.groupcondition
{
	public interface IAbstractGroupCondition : IQueryGroupCondition
	{
		String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

