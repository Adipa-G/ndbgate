using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractSqlQueryFrom : IAbstractFrom
	{
		public string Sql { get; set; }

		public QueryFromExpressionType FromExpressionType
		{
			get {return QueryFromExpressionType.RawSql;}
		}

		public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
			return Sql;
		}
	}
}

