﻿using dbgate.ermanagement;

namespace dbgate.support.patch.patchtabledifferences
{
    [TableInfo("table_change_test_entity")]
    public class ThreeColumnTypeDifferentEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar),Size = 255)]
        public string Name { get; set; }
    }
}