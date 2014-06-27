using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;
using DbGate.ErManagement.ErMapper;

namespace DbGate.Support.Persistant.TreeTest
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

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            ErManagement.ErMapper.DbGate.GetSharedInstance().Load(this, reader, con);
        }

        public void Persist(IDbConnection con)
        {
            ErManagement.ErMapper.DbGate.GetSharedInstance().Save(this, con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}