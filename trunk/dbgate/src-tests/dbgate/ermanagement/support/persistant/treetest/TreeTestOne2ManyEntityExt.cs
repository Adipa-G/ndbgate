using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.support.persistant.treetest
{
    public class TreeTestOne2ManyEntityExt : ITreeTestOne2ManyEntity
    {
        private IEntityContext _context;

        public DbClassStatus Status { get; set; }
        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string Name { get; set; }
        
        public TreeTestOne2ManyEntityExt()
        {
            Status = DbClassStatus.New;
            _context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            ErLayer.GetSharedInstance().Load(this,reader,con);
        }

        public void Persist(IDbConnection con)
        {
            ErLayer.GetSharedInstance().Save(this, con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}