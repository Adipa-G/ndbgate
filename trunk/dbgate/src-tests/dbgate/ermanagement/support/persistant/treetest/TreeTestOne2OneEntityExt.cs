using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.support.persistant.treetest
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
            ErLayer.GetSharedInstance().Load(this,reader,con);
        }

        public void Persist(IDbConnection con)
        {
           ErLayer.GetSharedInstance().Save(this,con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}