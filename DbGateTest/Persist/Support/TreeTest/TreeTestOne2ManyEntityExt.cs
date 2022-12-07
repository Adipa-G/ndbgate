﻿using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;

namespace DbGate.Persist.Support.TreeTest
{
    public class TreeTestOne2ManyEntityExt : ITreeTestOne2ManyEntity
    {
        private IEntityContext context;

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string Name { get; set; }
        
        public TreeTestOne2ManyEntityExt()
        {
            Status = EntityStatus.New;
            context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, ITransaction tx)
        {
            tx.DbGate.Load(this, reader, tx);
 
        }

        public void Persist(ITransaction tx)
        {
            tx.DbGate.Save(this, tx);
        }

        public IEntityContext Context => context;
    }
}