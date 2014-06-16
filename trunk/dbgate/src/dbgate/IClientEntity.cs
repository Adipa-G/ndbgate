namespace dbgate
{
    public interface IClientEntity : IReadOnlyClientEntity
    {
        EntityStatus Status { get; set; }
    }
}