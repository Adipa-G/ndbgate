using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition
{
    public class AbstractConditionFactory
    {
        public IAbstractCondition CreateCondition(QueryConditionExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryConditionExpressionType.RawSql:
                    return new AbstractSqlQueryCondition();
                case QueryConditionExpressionType.Expression:
                    return new AbstractExpressionCondition();
                default:
                    return null;
            }
        }
    }
}