using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition
{
	public class AbstractSqlQueryGroupCondition : IAbstractQueryGroupCondition
	{
		public String Sql { get; set; }

		public QueryGroupConditionExpressionType GroupConditionExpressionType
		{
			get {return QueryGroupConditionExpressionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

