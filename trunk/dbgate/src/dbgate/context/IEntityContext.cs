
namespace dbgate.context
{
    public interface IEntityContext
    {
        IChangeTracker ChangeTracker { get; }

        IErSession ErSession { get; set; }
    }
}
