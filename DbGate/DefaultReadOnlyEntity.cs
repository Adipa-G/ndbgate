using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.ErMapper;

namespace DbGate
{
    public class DefaultReadOnlyEntity : IReadOnlyEntity
    {
        private IEntityContext context;

        public DefaultReadOnlyEntity()
        {
            context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, ITransaction tx)
        {
            tx.DbGate.Load(this,reader,tx);
        }

        public IEntityContext Context => context;
    }
}