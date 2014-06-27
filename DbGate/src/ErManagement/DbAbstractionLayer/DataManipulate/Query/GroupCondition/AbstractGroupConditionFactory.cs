using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public class AbstractGroupConditionFactory
    {
        public IAbstractGroupCondition CreateGroupCondition(QueryGroupConditionExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryGroupConditionExpressionType.RawSql:
                    return new AbstractSqlQueryGroupCondition();
                case QueryGroupConditionExpressionType.Expression:
                    return new AbstractExpressionGroupCondition();
                default:
                    return null;
            }
        }
    }
}