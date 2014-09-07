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

        public void Retrieve(IDataReader reader, ITransaction tx)
        {
            tx.DbGate.Load(this,reader,tx);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}