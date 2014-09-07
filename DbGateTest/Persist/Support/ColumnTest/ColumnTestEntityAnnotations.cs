using System;

namespace DbGate.Persist.Support.ColumnTest
{
    [TableInfo("column_test_entity")]
    public class ColumnTestEntityAnnotations : DefaultEntity, IColumnTestEntity
    {
        #region IColumnTestEntity Members

        [ColumnInfo((ColumnType.Integer), Key = true, ReadFromSequence = true,SequenceGeneratorType = typeof(PrimaryKeyGenerator))]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Long)]
        public long LongNotNull { get; set; }

        [ColumnInfo((ColumnType.Long), Nullable = true)]
        public long? LongNull { get; set; }

        [ColumnInfo(ColumnType.Boolean)]
        public bool BooleanNotNull { get; set; }

        [ColumnInfo((ColumnType.Boolean), Nullable = true)]
        public bool? BooleanNull { get; set; }

        [ColumnInfo(ColumnType.Char)]
        public char CharNotNull { get; set; }

        [ColumnInfo((ColumnType.Char), Nullable = true)]
        public char? CharNull { get; set; }

        [ColumnInfo(ColumnType.Integer)]
        public int IntNotNull { get; set; }

        [ColumnInfo((ColumnType.Integer), Nullable = true)]
        public int? IntNull { get; set; }

        [ColumnInfo(ColumnType.Date)]
        public DateTime DateNotNull { get; set; }

        [ColumnInfo((ColumnType.Date), Nullable = true)]
        public DateTime? DateNull { get; set; }

        [ColumnInfo(ColumnType.Double)]
        public double DoubleNotNull { get; set; }

        [ColumnInfo((ColumnType.Double), Nullable = true)]
        public double? DoubleNull { get; set; }

        [ColumnInfo(ColumnType.Float)]
        public float FloatNotNull { get; set; }

        [ColumnInfo((ColumnType.Float), Nullable = true)]
        public float? FloatNull { get; set; }

        [ColumnInfo(ColumnType.Timestamp)]
        public DateTime TimestampNotNull { get; set; }

        [ColumnInfo((ColumnType.Timestamp), Nullable = true)]
        public DateTime? TimestampNull { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string VarcharNotNull { get; set; }

        [ColumnInfo((ColumnType.Varchar), Nullable = true)]
        public string VarcharNull { get; set; }

        #endregion
    }
}