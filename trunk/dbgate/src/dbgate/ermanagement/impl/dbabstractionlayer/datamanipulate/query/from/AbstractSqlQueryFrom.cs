using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractSqlQueryFrom : IAbstractQueryFrom
	{
		public String Sql { get; set; }

		public QueryFromExpressionType FromExpressionType
		{
			get {return QueryFromExpressionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

