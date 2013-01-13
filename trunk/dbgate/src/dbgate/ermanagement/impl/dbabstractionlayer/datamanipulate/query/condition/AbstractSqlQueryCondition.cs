using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractSqlQueryCondition : IAbstractQueryCondition
	{
		public String Sql { get; set; }
	 	
	 	public QueryConditionType ConditionType 
		{
			get {return QueryConditionType.RAW_SQL;}
		}
	 	
	 	public String CreateSql()
	 	{
	 		return Sql;
	 	}
	}
}

