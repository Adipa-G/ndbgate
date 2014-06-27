using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy
{
    public class AbstractOrderByFactory
    {
        public IAbstractOrderBy CreateOrderBy(QueryOrderByExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryOrderByExpressionType.RawSql:
                    return new AbstractSqlQueryOrderBy();
                case QueryOrderByExpressionType.Expression:
                    return new AbstractExpressionOrderBy();
                default:
                    return null;
            }
        }
    }
}