using System.Collections.Generic;

namespace DbGate.Utility.Support
{
    public class RootEntity : IClientEntity
    {
        public RootEntity()
        {
            LeafEntities = new List<LeafEntity>();
            Status = EntityStatus.Unmodified;
        }

        public LeafEntity LeafEntityNotNull { get; set; }
        public LeafEntity LeafEntityNull { get; set; }
        public List<LeafEntity> LeafEntities { get; set; }

        #region IClientEntity Members

        public EntityStatus Status { get; set; }

        #endregion
    }
}