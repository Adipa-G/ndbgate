using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public interface IAbstractCondition : IQueryCondition
	{
        string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

