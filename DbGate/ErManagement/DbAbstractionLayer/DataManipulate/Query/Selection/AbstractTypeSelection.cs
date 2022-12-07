using System;
using System.Data;
using DbGate.ErManagement.Query;
using DbGate.Exceptions;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection
{
	public class AbstractTypeSelection : IAbstractSelection
	{
		public Type EntityType { get; set; }

		public QuerySelectionExpressionType SelectionType => QuerySelectionExpressionType.EntityType;

        public string CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
		{
			var aliases = buildInfo.GetAlias(EntityType);
			if (!string.IsNullOrEmpty(aliases))
			{
				return aliases + ".*";
			}
			return "*";
		}

		public Object Retrieve (IDataReader rs, ITransaction tx,QueryBuildInfo buildInfo)
		{
			try 
			{
                var instance = (IReadOnlyEntity)Activator.CreateInstance(EntityType);
				instance.Retrieve(rs,tx);
				return instance;
			} 
			catch (Exception ex) 
			{
				throw new RetrievalException(ex.Message,ex);
			}
		}
	}
}

