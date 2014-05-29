using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dbgate.ermanagement.query;
using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractTypeSelection : IAbstractSelection
	{
		public Type EntityType { get; set; }

		public QuerySelectionExpressionType SelectionExpressionType
		{
			get {return QuerySelectionExpressionType.ENTITY_TYPE;}
		}
		
		public String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
		{
			var aliases = buildInfo.Aliases;
			if (aliases.ContainsValue(EntityType))
			{
				var keys = aliases.Keys;
				foreach (string key in keys)
				{
					if ((aliases[key] as Type) == EntityType)
					{
						return key + ".*";
					}
				}
			}
			return "*";
		}

		public Object Retrieve (IDataReader rs, IDbConnection con)
		{
			try 
			{
				IServerDbClass instance = (IServerDbClass)Activator.CreateInstance(EntityType);
				instance.Retrieve(rs,con);
				return instance;
			} 
			catch (Exception ex) 
			{
				throw new RetrievalException(ex.Message,ex);
			}
		}
	}
}

