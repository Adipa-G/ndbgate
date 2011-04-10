
namespace dbgate.ermanagement.context
{
    public interface IEntityFieldValueList : ITypeFieldValueList
    {
        IServerRoDbClass Entity { get; }
    }
}