using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractSqlQueryGroup : IAbstractGroup
	{
		public string Sql { get; set; }

		public QueryGroupExpressionType GroupExpressionType
		{
			get {return QueryGroupExpressionType.RawSql;}
		}

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
			return Sql;
		}
	}
}

