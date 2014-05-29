using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractSqlQueryGroup : IAbstractGroup
	{
		public String Sql { get; set; }

		public QueryGroupExpressionType GroupExpressionType
		{
			get {return QueryGroupExpressionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

