
namespace dbgate.ermanagement.context
{
    public interface IEntityFieldValueList : ITypeFieldValueList
    {
        IReadOnlyEntity Entity { get; }
    }
}