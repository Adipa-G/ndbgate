namespace DbGate.Support.Persistant.DirtyCheck
{
    [TableInfo("dirty_check_test_one2one")]
    public class DirtyCheckTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}