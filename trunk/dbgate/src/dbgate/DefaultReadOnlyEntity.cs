using System.Data;
using dbgate.context;
using dbgate.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate
{
    public class DefaultReadOnlyEntity : IReadOnlyEntity
    {
        private IEntityContext _context;

        public DefaultReadOnlyEntity()
        {
            _context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            DbGate.GetSharedInstance().Load(this,reader,con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}