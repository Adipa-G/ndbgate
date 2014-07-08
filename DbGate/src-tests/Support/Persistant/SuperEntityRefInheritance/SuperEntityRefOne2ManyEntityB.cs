namespace DbGate.Support.Persistant.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2many_b")]
    public class SuperEntityRefOne2ManyEntityB : SuperEntityRefOne2ManyEntity
    {
        [ColumnInfo((ColumnType.Varchar),Size = 100)]
        public virtual string NameB { get; set; }
    }
}