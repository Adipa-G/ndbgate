﻿using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public class InheritanceTestSubEntityAExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityA
    {
        public string NameA { get; set; }

        public InheritanceTestSubEntityAExt()
        {
        }

        public override void Persist(IDbConnection con)
        {
            ErLayer.GetSharedInstance().Save(this,con);
        }

        public override void Retrieve(IDataReader rs, IDbConnection con)
        {
            ErLayer.GetSharedInstance().Load(this,rs,con);
        }
    }
}