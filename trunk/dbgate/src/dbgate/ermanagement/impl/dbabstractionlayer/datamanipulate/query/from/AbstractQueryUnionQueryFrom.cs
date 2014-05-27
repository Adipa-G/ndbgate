/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/27/2014
 * Time: 11:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using dbgate.ermanagement;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.query;

namespace dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractQueryUnionQueryFrom : IAbstractQueryFrom
	{
		public ISelectionQuery[] Queries { get; set; }
		public bool All { get; set; }
	  	 	
  	 	public QueryFromExpressionType FromExpressionType
  	 	{
  	 		get { return QueryFromExpressionType.QUERY_UNION; }
  	 	}
  	 	
  	 	public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
  	 	{
  	 		var sqlBuilder = new StringBuilder();
	  	 	sqlBuilder.Append("(");
	  	 	for (int i = 0, queriesLength = Queries.Length; i < queriesLength; i++)
	  	 	{
		  	 	ISelectionQuery query = Queries[i];
		  	 	QueryBuildInfo result = dbLayer.GetDataManipulate().ProcessQuery(buildInfo, query.Structure);
		  	 	if (i > 0)
		  	 	{
		  	 		sqlBuilder.Append( " UNION ");
			  	 	if (All)
			  	 	{
			  	 		sqlBuilder.Append(" ALL ");
			  	 	}
		  	 	}
		  	 	sqlBuilder.Append( result.ExecInfo.Sql + " u_"+i );
	  	 	}
	  	 	sqlBuilder.Append(") src_tbl");
	  	 	return sqlBuilder.ToString();
  	 	}
	}
}
