using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition
{
	public interface IAbstractGroupCondition : IQueryGroupCondition
	{
		String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

