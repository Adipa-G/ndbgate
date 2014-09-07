using System.Data;

namespace DbGate.Persist.Support.InheritanceTest
{
    public class InheritanceTestSubEntityBExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityB
    {
        public string NameB { get; set; }

        public InheritanceTestSubEntityBExt()
        {
        }

        public override void Persist(ITransaction tx)
        {
            tx.DbGate.Save(this,tx);
        }

        public override void Retrieve(IDataReader rs, ITransaction tx)
        {
            tx.DbGate.Load(this,rs,tx);
        }
    }
}