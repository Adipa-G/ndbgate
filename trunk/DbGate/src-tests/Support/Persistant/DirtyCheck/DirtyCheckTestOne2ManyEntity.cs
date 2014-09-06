namespace DbGate.Support.Persistant.DirtyCheck
{
    [TableInfo("dirty_check_test_one2many")]
    public class DirtyCheckTestOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }
    }
}