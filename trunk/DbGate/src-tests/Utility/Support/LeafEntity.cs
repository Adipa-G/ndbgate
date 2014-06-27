namespace DbGate.Utility.Support
{
    public class LeafEntity : IClientEntity
    {
        public LeafEntity()
        {
            Status = EntityStatus.Unmodified;
        }

        #region IClientEntity Members

        public EntityStatus Status { get; set; }

        #endregion
    }
}