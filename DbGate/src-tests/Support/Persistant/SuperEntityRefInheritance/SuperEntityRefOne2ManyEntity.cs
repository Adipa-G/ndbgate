namespace DbGate.Support.Persistant.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_one2many")]
    public class SuperEntityRefOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true,SubClassCommonColumn = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true,SubClassCommonColumn = true)]
        public virtual int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar),Size = 100)]
        public virtual string Name { get; set; }
    }
}