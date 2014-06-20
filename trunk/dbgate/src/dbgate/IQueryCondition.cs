using dbgate.ermanagement.query;

namespace dbgate
{
    public interface IQueryCondition
    {
		QueryConditionExpressionType ConditionExpressionType { get; }
    }
}