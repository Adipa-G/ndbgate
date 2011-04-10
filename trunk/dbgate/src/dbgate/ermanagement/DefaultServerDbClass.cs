using System.Data;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement
{
    public class DefaultServerDbClass : DefaultServerRoDbClass, IServerDbClass
    {
        public DefaultServerDbClass()
        {
            Status = DbClassStatus.New;
        }

        #region IServerDbClass Members

        public DbClassStatus Status { get; set; }

        public void Persist(IDbConnection con)
        {
            ErLayer.GetSharedInstance().Save(this,con);
        }

        #endregion

        public void _modify()
        {
            if (Status == DbClassStatus.Unmodified)
            {
                Status = DbClassStatus.Modified;
            }
        }
    }
}