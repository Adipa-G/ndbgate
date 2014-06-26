/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/27/2014
 * Time: 11:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@from
{
	public class AbstractSubQueryFrom : IAbstractFrom
	{
		public ISelectionQuery Query { get; set; }
		public string Alias { get; set; }

	    public QueryFromExpressionType FromExpressionType
	    {
	    	get { return QueryFromExpressionType.Query; }
	    }

	    public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
	        QueryBuildInfo result = dbLayer.DataManipulate().ProcessQuery(buildInfo,Query.Structure);
	        string sql = "(" + result.ExecInfo.Sql + ")";
	        if (!string.IsNullOrEmpty(Alias))
	        {
	            sql = sql + " as " + Alias;
	            buildInfo.AddQueryAlias(Alias,Query);
	        }
	        return sql;
	    }
	}
}
