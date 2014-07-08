namespace DbGate.Support.Persistant.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2many_a")]
    public class SuperEntityRefOne2ManyEntityA : SuperEntityRefOne2ManyEntity
    {
        [ColumnInfo((ColumnType.Varchar),Size = 100)]
        public virtual string NameA { get; set; }
    }
}