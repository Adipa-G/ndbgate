
namespace dbgate.context
{
    public interface IEntityFieldValueList : ITypeFieldValueList
    {
        IReadOnlyEntity Entity { get; }
    }
}