﻿using System;

namespace dbgate.ermanagement
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnInfo : Attribute
    {
        public readonly DbColumnType ColumnType;

        public DbColumnInfo(DbColumnType columnType)
        {
            ColumnType = columnType;
        }

        public string ColumnName { get; set; }

        public bool Key { get; set; }

        public bool Nullable { get; set; }

        public bool SubClassCommonColumn { get; set; }

        public int Size { get; set; }

        public bool ReadFromSequence { get; set; }

        public string SequenceGeneratorClassName { get; set; }
    }
}