/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/31/2014
 * Time: 11:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data;
using dbgate.ermanagement;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using dbgate.ermanagement.query;

namespace dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public abstract class BaseColumnOperation : IAbstractSelection
	{
		protected string _function;
		public string Field { get; set; }
		public string Alias { get; set; }
		public Type EntityType { get; set; }
		
		public abstract QuerySelectionExpressionType SelectionType { get; }
	  	
	  	protected IDbColumn GetColumn(QueryBuildInfo buildInfo)
  		{
	  		ICollection<IDbColumn> columns = null;
	  		
	  		try
	  		{
	  			columns = CacheManager.FieldCache.GetDbColumns(EntityType);
	  		}
	  		catch (FieldCacheMissException)
	  		{
		  		try
		  		{
		  			CacheManager.FieldCache.Register(EntityType,(IServerRoDbClass) Activator.CreateInstance(EntityType));
		  			columns = CacheManager.FieldCache.GetDbColumns(EntityType);
		  		}
		  		catch (Exception ex)
		  		{
		  			Console.Write(ex.StackTrace);
		  		}
  			}
	  		
	  		foreach (IDbColumn column in columns)
	  		{
	  			if (column.AttributeName.Equals(Field))
	  			{
	  				return column;
	  			}
	  		}
  			return null;
  		}
	  		
  		public String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
  		{
	  		String tableAlias = buildInfo.GetAlias(EntityType);
	  		tableAlias = (tableAlias == null)?"" : tableAlias + ".";
	  		IDbColumn column = GetColumn(buildInfo);
	  	 	
	  	 	if (column != null)
	  	 	{
	  	 		String sql = "";
	  	 		if (!string.IsNullOrEmpty(_function))
		  	 	{
		  	 		sql = _function + "(" +tableAlias + column.ColumnName+ ")";
	  	 		}
	  	 		else
	  	 		{
	  	 			sql = tableAlias + column.ColumnName;
	  	 		}
	  	 		if (!string.IsNullOrEmpty(Alias))
	  	 		{
	  	 			sql = sql + " AS " + Alias;
	  	 		}
	  	 		return sql;
	  	 	}
	  	 	else
	  	 	{
	  	 		return "<incorrect column for " + Field + ">";
	  	 	}
  	 	}
	  	 	
  	 	public Object Retrieve(IDataReader rs, IDbConnection con,QueryBuildInfo buildInfo)
  	 	{
	  	 	try
	  	 	{
	  	 		String columnName = !string.IsNullOrEmpty(Alias)? Alias : GetColumn(buildInfo).ColumnName;
		  	 	int ordinal = rs.GetOrdinal(columnName);
		  	 	var value = rs.GetValue(ordinal);
		  	 	return value;
	  	 	}
	  	 	catch (Exception ex)
	  	 	{
	  	 		throw new RetrievalException(ex.Message,ex);
	  	 	}
  	 	}
	}
}
