using System;

namespace dbgate.ermanagement.support.persistant.columntest
{
    [DbTableInfo("column_test_entity")]
    public class ColumnTestEntityAnnotations : DefaultServerDbClass , IColumnTestEntity
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true, ReadFromSequence = true, SequenceGeneratorClassName = "dbgate.ermanagement.support.persistant.columntest.PrimaryKeyGenerator")]
        public int IdCol { get; set; }
        
        [DbColumnInfo(DbColumnType.Long)]
        public long LongNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Long),Nullable = true)]
        public long? LongNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Boolean)]
        public bool BooleanNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Boolean),Nullable = true)]
        public bool? BooleanNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Char)]
        public char CharNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Char),Nullable = true)]
        public char? CharNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Integer)]
        public int IntNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Integer),Nullable = true)]
        public int? IntNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Date)]
        public DateTime DateNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Date),Nullable = true)]
        public DateTime? DateNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Double)]
        public double DoubleNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Double),Nullable = true)]
        public double? DoubleNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Float)]
        public float FloatNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Float),Nullable = true)]
        public float? FloatNull { get; set; }
       
        [DbColumnInfo(DbColumnType.Timestamp)]
        public DateTime TimestampNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Timestamp),Nullable = true)]
        public DateTime? TimestampNull { get; set; }
        
        [DbColumnInfo(DbColumnType.Varchar)]
        public string VarcharNotNull { get; set; }
        [DbColumnInfo((DbColumnType.Varchar),Nullable = true)]
        public string VarcharNull { get; set; }
    }
}