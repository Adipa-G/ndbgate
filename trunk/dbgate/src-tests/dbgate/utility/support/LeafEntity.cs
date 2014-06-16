namespace dbgate.utility.support
{
    public class LeafEntity : IClientEntity
    {
        public EntityStatus Status { get; set; }

        public LeafEntity()
        {
            Status = EntityStatus.Unmodified;
        }
    }
}
