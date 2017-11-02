namespace DbGate.Persist.Support.CrossReference
{
    [TableInfo("cross_reference_test_one2many")]
    public class CrossReferenceTestOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_one2oneent2root",
            typeof (CrossReferenceTestRootEntity),
            new[] {"idCol"},
            new[] {"idCol"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public CrossReferenceTestRootEntity RootEntity { get; set; }
    }
}