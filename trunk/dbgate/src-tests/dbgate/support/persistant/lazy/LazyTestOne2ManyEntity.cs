using dbgate.ermanagement;

namespace dbgate.support.persistant.lazy
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