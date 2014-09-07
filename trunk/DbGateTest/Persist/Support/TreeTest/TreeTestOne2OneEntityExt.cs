using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;

namespace DbGate.Persist.Support.TreeTest
{
    public class TreeTestOne2OneEntityExt : ITreeTestOne2OneEntity
    {
        private IEntityContext _context;

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public string Name { get; set; }

        public TreeTestOne2OneEntityExt()
        {
            Status = EntityStatus.New;
            _context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, ITransaction tx)
        {
            tx.DbGate.Load(this,reader,tx);
        }

        public void Persist(ITransaction tx)
        {
            tx.DbGate.Save(this,tx);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}