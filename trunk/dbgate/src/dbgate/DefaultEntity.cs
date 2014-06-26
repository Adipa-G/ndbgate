using System.Data;
using dbgate.ermanagement.ermapper;

namespace dbgate
{
    public class DefaultEntity : DefaultReadOnlyEntity, IEntity
    {
        public DefaultEntity()
        {
            Status = EntityStatus.New;
        }

        #region IEntity Members

        public EntityStatus Status { get; set; }

        public void Persist(IDbConnection con)
        {
            DbGate.GetSharedInstance().Save(this,con);
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