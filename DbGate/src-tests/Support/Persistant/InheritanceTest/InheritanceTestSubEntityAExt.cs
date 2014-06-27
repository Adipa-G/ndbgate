using System.Data;
using DbGate.ErManagement.ErMapper;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSubEntityAExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityA
    {
        public string NameA { get; set; }

        public InheritanceTestSubEntityAExt()
        {
        }

        public override void Persist(IDbConnection con)
        {
            ErManagement.ErMapper.DbGate.GetSharedInstance().Save(this, con);
        }

        public override void Retrieve(IDataReader rs, IDbConnection con)
        {
            ErManagement.ErMapper.DbGate.GetSharedInstance().Load(this, rs, con);
        }
    }
}