using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public class AbstractGroupFactory
    {
        public IAbstractGroup CreateGroup(QueryGroupExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryGroupExpressionType.RawSql:
                    return new AbstractSqlQueryGroup();
                case QueryGroupExpressionType.Expression:
                    return new AbstractExpressionGroup();
                default:
                    return null;
            }
        }
    }
}