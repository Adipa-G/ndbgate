using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dbgate.ermanagement.query;
using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractTypeQuerySelection : IAbstractQuerySelection
	{
		public Type EntityType { get; set; }

		public QuerySelectionExpressionType SelectionExpressionType
		{
			get {return QuerySelectionExpressionType.ENTITY_TYPE;}
		}
		
		public String CreateSql(QueryBuildInfo buildInfo)
		{
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

