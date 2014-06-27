using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.ErMapper;

namespace DbGate
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
            ErManagement.ErMapper.DbGate.GetSharedInstance().Load(this, reader, con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}