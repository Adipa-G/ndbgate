using System;
using dbgate.ermanagement.query;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractTypeFrom : IAbstractFrom
	{
		public Type EntityType { get; set; }
		public string Alias { get; set; }

		public QueryFromExpressionType FromExpressionType
		{
			get {return QueryFromExpressionType.EntityType;}
		}
		
		public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
	    {
	        string sql = null;
	        try
	        {
	            sql = CacheManager.TableCache.GetTableName(EntityType);
	        }
	        catch (TableCacheMissException)
	        {
	            try
	            {
	                ErDataManagerUtils.RegisterType(EntityType);
	                sql = CacheManager.TableCache.GetTableName(EntityType);
	            }
	            catch (Exception ex)
	            {
	                throw new QueryBuildingException(string.Format("unable to create from segment for type {0}",EntityType.FullName),ex);
	            }
	            return "<unknown " + EntityType.FullName + ">";
	        }
	        
	        if (!string.IsNullOrEmpty(Alias))
	        {
	            sql = sql + " as " + Alias;
	            buildInfo.AddTypeAlias(Alias,EntityType);
	        }
	        return sql;
	    }
	}
}

