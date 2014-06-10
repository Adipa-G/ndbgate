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
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractUnionFrom : IAbstractFrom
	{
		public ISelectionQuery[] Queries { get; set; }
		public bool All { get; set; }
	  	 	
  	 	public QueryFromExpressionType FromExpressionType
  	 	{
  	 		get { return QueryFromExpressionType.QueryUnion; }
  	 	}
  	 	
  	 	public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
  	 	{
  	 		String alias = "union_src_" + Guid.NewGuid().ToString().Substring(0,5);
  	 		
  	 		var sqlBuilder = new StringBuilder();
	  	 	sqlBuilder.Append("(");
	  	 	for (int i = 0, queriesLength = Queries.Length; i < queriesLength; i++)
	  	 	{
		  	 	ISelectionQuery query = Queries[i];
		  	 	QueryBuildInfo result = dbLayer.DataManipulate().ProcessQuery(buildInfo, query.Structure);
		  	 	if (i > 0)
		  	 	{
		  	 		sqlBuilder.Append( " UNION ");
			  	 	if (All)
			  	 	{
			  	 		sqlBuilder.Append(" ALL ");
			  	 	}
		  	 	}
		  	 	sqlBuilder.Append( result.ExecInfo.Sql + " union_src_"+i );
	  	 	}
	  	 	
	  	 	sqlBuilder.Append(") ").Append(alias);
	  	 	buildInfo.AddUnionAlias(alias);
	  	 	
	  	 	return sqlBuilder.ToString();
  	 	}
	}
}
