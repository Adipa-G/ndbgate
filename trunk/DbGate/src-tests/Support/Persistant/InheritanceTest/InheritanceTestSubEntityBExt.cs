using System.Data;
using DbGate.ErManagement.ErMapper;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSubEntityBExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityB
    {
        public string NameB { get; set; }

        public InheritanceTestSubEntityBExt()
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