namespace DbGate.Persist.Support.Lazy
{
    [TableInfo("lazy_test_one2one")]
    public class LazyTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }
    }
}