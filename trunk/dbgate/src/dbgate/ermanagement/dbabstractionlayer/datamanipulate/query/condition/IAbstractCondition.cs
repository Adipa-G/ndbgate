using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.condition
{
	public interface IAbstractCondition : IQueryCondition
	{
        string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

