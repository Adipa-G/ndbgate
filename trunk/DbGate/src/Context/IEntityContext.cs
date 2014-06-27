namespace DbGate.Context
{
    public interface IEntityContext
    {
        IChangeTracker ChangeTracker { get; }

        IErSession ErSession { get; set; }
    }
}