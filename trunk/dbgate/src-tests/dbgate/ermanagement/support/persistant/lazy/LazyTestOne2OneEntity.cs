namespace dbgate.ermanagement.support.persistant.lazy
{
    [DbTableInfo("lazy_test_one2one")]
    public class LazyTestOne2OneEntity : DefaultServerDbClass
    {
        public LazyTestOne2OneEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public virtual string Name { get; set; }
    }
}