using System;
using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public interface IAbstractQuerySelection : IQuerySelection
	{
		String CreateSql(QueryExecInfo execInfo);

		Object Retrieve(IDataReader rs,IDbConnection con);
	}
}

