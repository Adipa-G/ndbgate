namespace dbgate.ermanagement.support.persistant.lazy
{
    [DbTableInfo("lazy_test_one2many")]
    public class LazyTestOne2ManyEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public virtual int IndexNo { get; set; }

        [DbColumnInfo((DbColumnType.Varchar))]
        public virtual string Name { get; set; }
    }
}
