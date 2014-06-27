namespace DbGate.Support.Patch.PatchTableDifferences
{
    [TableInfo("table_change_test_entity")]
    public class FourColumnEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Code { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }
    }
}