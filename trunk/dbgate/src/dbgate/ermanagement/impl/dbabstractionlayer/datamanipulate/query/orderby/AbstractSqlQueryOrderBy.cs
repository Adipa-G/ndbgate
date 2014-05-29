using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby
{
	public class AbstractSqlQueryOrderBy : IAbstractOrderBy
	{
		public String Sql { get; set; }

		public QueryOrderByExpressionType OrderByExpressionType
		{
			get {return QueryOrderByExpressionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

