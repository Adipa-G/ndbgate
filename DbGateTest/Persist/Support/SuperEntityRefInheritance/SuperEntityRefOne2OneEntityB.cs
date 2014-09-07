namespace DbGate.Persist.Support.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2one_b")]
    public class SuperEntityRefOne2OneEntityB : SuperEntityRefOne2OneEntity
    {
        [ColumnInfo((ColumnType.Varchar))]
        public virtual string NameB { get; set; }
    }
}