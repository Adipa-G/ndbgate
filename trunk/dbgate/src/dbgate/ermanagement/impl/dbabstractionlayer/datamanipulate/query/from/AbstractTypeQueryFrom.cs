using System;
using dbgate.ermanagement.query;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractTypeQueryFrom : IAbstractQueryFrom
	{
		public Type EntityType { get; set; }

		public QueryFromExpressionType FromExpressionType
		{
			get {return QueryFromExpressionType.ENTITY_TYPE;}
		}
		
		public String CreateSql()
		{
			try
	  	 	{
				return CacheManager.TableCache.GetTableName(EntityType);
	  	 	}
			catch (TableCacheMissException e)
	  	 	{
		  	 	try
		  	 	{
					ErDataManagerUtils.RegisterTypes((IServerRoDbClass)Activator.CreateInstance(EntityType));
					return CacheManager.TableCache.GetTableName(EntityType);	  	 			
		  	 	}
		  	 	catch (Exception ex)
		  	 	{
					throw new QueryBuildingException(string.Format("unable to create from segment for type {0}",EntityType.FullName),ex);
		  	 		return "<unknown " + EntityType.FullName + ">";
		  	 	}
			}
		}
	}
}

