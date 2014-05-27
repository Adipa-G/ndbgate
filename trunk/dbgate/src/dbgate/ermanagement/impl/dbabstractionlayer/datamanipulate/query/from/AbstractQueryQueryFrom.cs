/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/27/2014
 * Time: 11:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using dbgate.ermanagement;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.query;

namespace dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractQueryQueryFrom : IAbstractQueryFrom
	{
		public ISelectionQuery Query { get; set; }
		public String Alias { get; set; }

	    public QueryFromExpressionType FromExpressionType
	    {
	    	get { return QueryFromExpressionType.QUERY; }
	    }

	    public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
	        QueryBuildInfo result = dbLayer.GetDataManipulate().ProcessQuery(buildInfo,Query.Structure);
	        String sql = "(" + result.ExecInfo.Sql + ")";
	        if (!string.IsNullOrEmpty(Alias))
	        {
	            sql = sql + " as " + Alias;
	            buildInfo.AddQueryAlias(Alias,Query);
	        }
	        return sql;
	    }
	}
}
