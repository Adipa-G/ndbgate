﻿using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public class InheritanceTestSubEntityBExt : InheritanceTestSuperEntityExt , IInheritanceTestSubEntityB
    {
        public string NameB { get; set; }

        public InheritanceTestSubEntityBExt()
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