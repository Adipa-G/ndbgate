﻿using System.Data;
using dbgate.context;
using dbgate.context.impl;
using dbgate.ermanagement.ermapper;

namespace dbgate.support.persistant.treetest
{
    public class TreeTestOne2ManyEntityExt : ITreeTestOne2ManyEntity
    {
        private IEntityContext _context;

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string Name { get; set; }
        
        public TreeTestOne2ManyEntityExt()
        {
            Status = EntityStatus.New;
            _context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            DbGate.GetSharedInstance().Load(this,reader,con);
        }

        public void Persist(IDbConnection con)
        {
            DbGate.GetSharedInstance().Save(this, con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}