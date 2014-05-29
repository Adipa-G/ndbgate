using System;
using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public interface IAbstractSelection : IQuerySelection
	{
		String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo);

		Object Retrieve(IDataReader rs,IDbConnection con);
	}
}

