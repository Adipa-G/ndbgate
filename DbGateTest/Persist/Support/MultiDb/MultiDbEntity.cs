namespace DbGate.Persist.Support.MultiDb
{
    [TableInfo("multi_db_test_root")]
    public class MultiDbEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }
    }
}
