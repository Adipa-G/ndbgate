using System.Data;
using DbGate.ErManagement.ErMapper;

namespace DbGate
{
    public class DefaultEntity : DefaultReadOnlyEntity, IEntity
    {
        public DefaultEntity()
        {
            Status = EntityStatus.New;
        }

        #region IEntity Members

        public EntityStatus Status { get; set; }

        public void Persist(ITransaction tx)
        {
            tx.DbGate.Save(this,tx);
        }

        #endregion

        public void _modify()
        {
            if (Status == EntityStatus.Unmodified)
            {
                Status = EntityStatus.Modified;
            }
        }
    }
}