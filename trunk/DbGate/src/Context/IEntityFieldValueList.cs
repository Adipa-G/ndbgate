namespace DbGate.Context
{
    public interface IEntityFieldValueList : ITypeFieldValueList
    {
        IReadOnlyEntity Entity { get; }
    }
}