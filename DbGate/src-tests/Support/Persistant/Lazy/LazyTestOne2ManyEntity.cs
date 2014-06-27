namespace DbGate.Support.Persistant.Lazy
{
    [TableInfo("lazy_test_one2many")]
    public class LazyTestOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public virtual string Name { get; set; }
    }
}