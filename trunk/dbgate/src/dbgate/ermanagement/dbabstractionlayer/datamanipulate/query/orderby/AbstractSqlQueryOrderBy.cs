using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@orderby
{
	public class AbstractSqlQueryOrderBy : IAbstractOrderBy
	{
		public string Sql { get; set; }

		public QueryOrderByExpressionType OrderByExpressionType
		{
			get {return QueryOrderByExpressionType.RawSql;}
		}

	    public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
	        return Sql;
		}
	}
}

