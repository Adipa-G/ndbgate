using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@join
{
	public class AbstractSqlQueryJoin : IAbstractJoin
	{
		public string Sql { get; set; }

		public QueryJoinExpressionType JoinExpressionType
		{
			get {return QueryJoinExpressionType.RawSql;}
		}

	    public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
			return Sql;
		}
	}
}

