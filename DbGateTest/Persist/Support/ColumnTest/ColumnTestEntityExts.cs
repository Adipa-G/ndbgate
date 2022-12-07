using System;
using System.Data;
using DbGate.Context;
using DbGate.Context.Impl;

namespace DbGate.Persist.Support.ColumnTest
{
    public class ColumnTestEntityExts : IColumnTestEntity
    {
        private IEntityContext context;

        public ColumnTestEntityExts()
        {
            context = new EntityContext();
        }

        public EntityStatus Status { get; set; }
        public int IdCol { get; set; }
        public long LongNotNull { get; set; }
        public long? LongNull { get; set; }
        public bool BooleanNotNull { get; set; }
        public bool? BooleanNull { get; set; }
        public char CharNotNull { get; set; }
        public char? CharNull { get; set; }
        public int IntNotNull { get; set; }
        public int? IntNull { get; set; }
        public DateTime DateNotNull { get; set; }
        public DateTime? DateNull { get; set; }
        public double DoubleNotNull { get; set; }
        public double? DoubleNull { get; set; }
        public float FloatNotNull { get; set; }
        public float? FloatNull { get; set; }
        public DateTime TimestampNotNull { get; set; }
        public DateTime? TimestampNull { get; set; }
        public string VarcharNotNull { get; set; }
        public string VarcharNull { get; set; }
        public Guid GuidNotNull { get; set; }
        public Guid? GuidNull { get; set; }

        public void Persist(ITransaction tx)
        {
            tx.DbGate.Save(this, tx);
        }

        public void Retrieve(IDataReader reader, ITransaction tx)
        {
            tx.DbGate.Load(this, reader, tx);
        }

        public IEntityContext Context => context;
    }
}