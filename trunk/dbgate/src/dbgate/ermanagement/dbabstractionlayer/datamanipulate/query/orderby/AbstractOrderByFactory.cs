using dbgate.ermanagement.query;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@orderby
{
	public class AbstractOrderByFactory
	{
		public IAbstractOrderBy CreateOrderBy (QueryOrderByExpressionType expressionType)
		{
			switch (expressionType) 
			{
				case QueryOrderByExpressionType.RawSql:
					return new AbstractSqlQueryOrderBy ();
                case QueryOrderByExpressionType.Expression:
                    return new AbstractExpressionOrderBy();
				default:
					return null;
			}
		}
	}
}

