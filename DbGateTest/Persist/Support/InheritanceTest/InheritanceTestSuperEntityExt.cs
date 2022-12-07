using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;

namespace DbGate.Persist.Support.InheritanceTest
{
    public abstract class InheritanceTestSuperEntityExt : IInheritanceTestSuperEntity
    {
        private readonly IEntityContext context;

        protected InheritanceTestSuperEntityExt()
        {
            Status = EntityStatus.New;
            context = new EntityContext();
        }

        #region IInheritanceTestSuperEntity Members

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public string Name { get; set; }

        public abstract void Retrieve(IDataReader reader, ITransaction tx);

        public abstract void Persist(ITransaction tx);

        public IEntityContext Context => context;

        #endregion
    }
}