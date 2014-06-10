using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition
{
	public class AbstractSqlQueryCondition : IAbstractCondition
	{
		public string Sql { get; set; }
	 	
		public QueryConditionExpressionType ConditionExpressionType 
		{
			get {return QueryConditionExpressionType.RawSql;}
		}

	    public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
	        return Sql;
	 	}
	}
}

