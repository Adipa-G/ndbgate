using dbgate.ermanagement;

namespace dbgate.support.persistant.lazy
{
    [TableInfo("lazy_test_one2one")]
    public class LazyTestOne2OneEntity : DefaultEntity
    {
        public LazyTestOne2OneEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }
    }
}