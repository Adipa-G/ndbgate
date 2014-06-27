using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join
{
    public class AbstractJoinFactory
    {
        public IAbstractJoin CreateJoin(QueryJoinExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryJoinExpressionType.RawSql:
                    return new AbstractSqlQueryJoin();
                case QueryJoinExpressionType.Type:
                    return new AbstractTypeJoin();
                default:
                    return null;
            }
        }
    }
}