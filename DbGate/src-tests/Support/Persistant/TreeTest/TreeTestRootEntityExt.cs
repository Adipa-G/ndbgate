using System.Collections.Generic;
using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.ErMapper;

namespace DbGate.Support.Persistant.TreeTest
{
    public class TreeTestRootEntityExt : ITreeTestRootEntity
    {
        private IEntityContext _context;

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public string Name { get; set; }
        public List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }
        public ITreeTestOne2OneEntity One2OneEntity { get; set; }
        
        public TreeTestRootEntityExt()
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