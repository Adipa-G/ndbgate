namespace DbGate.Persist.Support.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2one")]
    public class SuperEntityRefOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true,SubClassCommonColumn = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public virtual string Name { get; set; }
    }
}