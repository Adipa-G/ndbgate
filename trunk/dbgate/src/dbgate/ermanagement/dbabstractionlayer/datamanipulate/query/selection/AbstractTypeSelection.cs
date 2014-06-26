using System;
using System.Data;
using dbgate.ermanagement.query;
using dbgate.exceptions;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractTypeSelection : IAbstractSelection
	{
		public Type EntityType { get; set; }

		public QuerySelectionExpressionType SelectionType
		{
			get {return QuerySelectionExpressionType.EntityType;}
		}
		
		public string CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
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
                var instance = (IReadOnlyEntity)Activator.CreateInstance(EntityType);
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

