using dbgate.ermanagement.query;

namespace dbgate
{
    public interface IQuerySelection
    {
		QuerySelectionExpressionType SelectionType { get; }
    }
}