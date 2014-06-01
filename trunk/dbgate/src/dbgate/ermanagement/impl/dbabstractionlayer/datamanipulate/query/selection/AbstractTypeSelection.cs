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

		public QuerySelectionExpressionType SelectionType
		{
			get {return QuerySelectionExpressionType.EntityType;}
		}
		
		public String CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
		{
			var aliases = buildInfo.GetAlias(EntityType);
			if (!string.IsNullOrEmpty(aliases))
			{
				return aliases + ".*";
			}
			return "*";
		}

		public Object Retrieve (IDataReader rs, IDbConnection con,QueryBuildInfo buildInfo)
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

