namespace DbGate
{
    public interface IClientEntity : IReadOnlyClientEntity
    {
        EntityStatus Status { get; set; }
    }
}