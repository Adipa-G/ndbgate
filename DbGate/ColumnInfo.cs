using System;

namespace DbGate
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnInfo : Attribute
    {
        public readonly ColumnType ColumnType;

        public ColumnInfo(ColumnType columnType)
        {
            ColumnType = columnType;
        }

        public string ColumnName { get; set; }

        public bool Key { get; set; }

        public bool Nullable { get; set; }

        public bool SubClassCommonColumn { get; set; }

        public int Size { get; set; }

        public bool ReadFromSequence { get; set; }

        public Type SequenceGeneratorType { get; set; }
    }
}