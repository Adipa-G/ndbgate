using System.Data;
using dbgate.ermanagement.impl;

namespace dbgate.support.persistant.inheritancetest
{
    public class InheritanceTestSubEntityBExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityB
    {
        public string NameB { get; set; }

        public InheritanceTestSubEntityBExt()
        {
        }

        public override void Persist(IDbConnection con)
        {
            DbGate.GetSharedInstance().Save(this,con);
        }

        public override void Retrieve(IDataReader rs, IDbConnection con)
        {
            DbGate.GetSharedInstance().Load(this,rs,con);
        }
    }
}