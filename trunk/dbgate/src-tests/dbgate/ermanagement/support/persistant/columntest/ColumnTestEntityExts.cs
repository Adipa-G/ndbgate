using System;
using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.support.persistant.columntest
{
    public class ColumnTestEntityExts : IColumnTestEntity
    {
        private IEntityContext _context;

        public ColumnTestEntityExts()
        {
            _context = new EntityContext();
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

        public void Persist(IDbConnection con)
        {
            DbGate.GetSharedInstance().Save(this,con);
        }

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            DbGate.GetSharedInstance().Load(this,reader,con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}