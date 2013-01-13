using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IQueryCondition
    {
		QueryConditionExpressionType ConditionExpressionType { get; }
    }
}