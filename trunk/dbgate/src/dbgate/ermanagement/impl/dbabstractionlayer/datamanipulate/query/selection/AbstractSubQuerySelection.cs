using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dbgate.ermanagement.query;
using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractSubQuerySelection : IAbstractSelection
	{
		public ISelectionQuery Query { get; set; }
		public string Alias { get; set; }

		public QuerySelectionExpressionType SelectionType
		{
			get {return QuerySelectionExpressionType.QUERY;}
		}
		
		public String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
		{
			QueryBuildInfo result = dbLayer.DataManipulate().ProcessQuery(buildInfo,Query.Structure);
			String sql = "(" + result.ExecInfo.Sql + ")";
			if (string.IsNullOrEmpty(Alias))
			{
				Alias = "col_" + Guid.NewGuid().ToString().Substring(0,5);
			}
			sql = sql + " as " + Alias;
			buildInfo.AddQueryAlias(Alias,Query);
			return sql;
		}

		public Object Retrieve (IDataReader rs,IDbConnection con,QueryBuildInfo buildInfo)
		{
			try
			{
				int ordinal = rs.GetOrdinal(Alias);
				Object obj = rs.GetValue(ordinal);
				return obj;
			} 
			catch (Exception ex) 
			{
				throw new RetrievalException(ex.Message,ex);
			}
		}
	}
}

