using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@orderby
{
	public interface IAbstractOrderBy : IQueryOrderBy
	{
	    String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
	}
}

