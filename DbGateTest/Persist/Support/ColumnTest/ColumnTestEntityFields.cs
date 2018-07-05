using System;
using System.Collections.Generic;

namespace DbGate.Persist.Support.ColumnTest
{
    public class ColumnTestEntityFields : AbstractManagedEntity, IColumnTestEntity
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (ColumnTestEntityFields), new DefaultTable("column_test_entity"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = new Dictionary<Type, ICollection<IField>>();
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("IdCol", "id_col", true, ColumnType.Integer, true,
                                                new PrimaryKeyGenerator()));
                dbColumns.Add(new DefaultColumn("LongNotNull", ColumnType.Long));
                dbColumns.Add(new DefaultColumn("LongNull", ColumnType.Long, true));
                dbColumns.Add(new DefaultColumn("BooleanNotNull", ColumnType.Boolean));
                dbColumns.Add(new DefaultColumn("BooleanNull", ColumnType.Boolean, true));
                dbColumns.Add(new DefaultColumn("CharNotNull", ColumnType.Char));
                dbColumns.Add(new DefaultColumn("CharNull", ColumnType.Char, true));
                dbColumns.Add(new DefaultColumn("IntNotNull", ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("IntNull", ColumnType.Integer, true));
                dbColumns.Add(new DefaultColumn("DateNotNull", ColumnType.Date));
                dbColumns.Add(new DefaultColumn("DateNull", ColumnType.Date, true));
                dbColumns.Add(new DefaultColumn("DoubleNotNull", ColumnType.Double));
                dbColumns.Add(new DefaultColumn("DoubleNull", ColumnType.Double, true));
                dbColumns.Add(new DefaultColumn("FloatNotNull", ColumnType.Float));
                dbColumns.Add(new DefaultColumn("FloatNull", ColumnType.Float, true));
                dbColumns.Add(new DefaultColumn("TimestampNotNull", ColumnType.Timestamp));
                dbColumns.Add(new DefaultColumn("TimestampNull", ColumnType.Timestamp, true));
                dbColumns.Add(new DefaultColumn("VarcharNotNull", ColumnType.Varchar));
                dbColumns.Add(new DefaultColumn("VarcharNull", ColumnType.Varchar, true));
                dbColumns.Add(new DefaultColumn("GuidNotNull", ColumnType.Guid));
                dbColumns.Add(new DefaultColumn("GuidNull", ColumnType.Guid, true));

                map.Add(typeof (ColumnTestEntityFields), dbColumns);
                return map;
            }
        }

        #region IColumnTestEntity Members

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

        #endregion
    }
}