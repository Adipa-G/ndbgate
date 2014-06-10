using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby
{
	public interface IAbstractOrderBy : IQueryOrderBy
	{
	    String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

