using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractSqlQueryGroup : IAbstractGroup
	{
		public String Sql { get; set; }

		public QueryGroupExpressionType GroupExpressionType
		{
			get {return QueryGroupExpressionType.RawSql;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

