using System;
using dbgate.caches;
using dbgate.caches.impl;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@from
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
            EntityInfo entityInfo = CacheManager.GetEntityInfo(EntityType);
            string sql = entityInfo.TableName;

	        if (!string.IsNullOrEmpty(Alias))
	        {
	            sql = sql + " as " + Alias;
	            buildInfo.AddTypeAlias(Alias,EntityType);
	        }
	        return sql;
	    }
	}
}

