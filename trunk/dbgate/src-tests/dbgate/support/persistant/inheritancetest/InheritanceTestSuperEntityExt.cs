using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;

namespace dbgate.support.persistant.inheritancetest
{
    public abstract class InheritanceTestSuperEntityExt : IInheritanceTestSuperEntity
    {
        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public string Name { get; set; }

        private IEntityContext _context;

        protected InheritanceTestSuperEntityExt()
        {
            Status = EntityStatus.New;
            _context = new EntityContext();
        }

        public abstract void Retrieve(IDataReader reader, IDbConnection con);

        public abstract void Persist(IDbConnection con);

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}