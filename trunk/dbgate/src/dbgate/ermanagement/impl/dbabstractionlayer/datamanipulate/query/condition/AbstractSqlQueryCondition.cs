using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractSqlQueryCondition : IAbstractCondition
	{
		public String Sql { get; set; }
	 	
		public QueryConditionExpressionType ConditionExpressionType 
		{
			get {return QueryConditionExpressionType.RAW_SQL;}
		}
	 	
	 	public String CreateSql()
	 	{
	 		return Sql;
	 	}
	}
}

