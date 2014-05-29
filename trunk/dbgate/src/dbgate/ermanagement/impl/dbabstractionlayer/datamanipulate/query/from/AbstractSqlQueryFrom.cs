using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractSqlQueryFrom : IAbstractFrom
	{
		public String Sql { get; set; }

		public QueryFromExpressionType FromExpressionType
		{
			get {return QueryFromExpressionType.RAW_SQL;}
		}

		public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
			return Sql;
		}
	}
}

