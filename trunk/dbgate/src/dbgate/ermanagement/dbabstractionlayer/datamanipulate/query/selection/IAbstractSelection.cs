using System;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.selection
{
	public interface IAbstractSelection : IQuerySelection
	{
		String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo);

		Object Retrieve(IDataReader rs,IDbConnection con,QueryBuildInfo buildInfo);
	}
}

