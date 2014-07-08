namespace DbGate.Support.Persistant.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2one_a")]
    public class SuperEntityRefOne2OneEntityA : SuperEntityRefOne2OneEntity
    {
        [ColumnInfo((ColumnType.Varchar))]
        public virtual string NameA { get; set; }
    }
}