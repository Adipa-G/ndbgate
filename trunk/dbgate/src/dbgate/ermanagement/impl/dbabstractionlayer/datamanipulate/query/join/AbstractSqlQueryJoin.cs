using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join
{
	public class AbstractSqlQueryJoin : IAbstractQueryJoin
	{
		public String Sql { get; set; }

		public QueryJoinExpressionType JoinExpressionType
		{
			get {return QueryJoinExpressionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

