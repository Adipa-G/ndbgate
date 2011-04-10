namespace dbgate.utility.support
{
    public class LeafEntity : IDbClass
    {
        public DbClassStatus Status { get; set; }

        public LeafEntity()
        {
            Status = DbClassStatus.Unmodified;
        }
    }
}
