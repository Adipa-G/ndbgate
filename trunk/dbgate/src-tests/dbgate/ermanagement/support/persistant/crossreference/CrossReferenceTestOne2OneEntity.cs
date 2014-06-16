namespace dbgate.ermanagement.support.persistant.crossreference
{
    [TableInfo("cross_reference_test_one2one")]
    public class CrossReferenceTestOne2OneEntity : DefaultEntity
    {
        public CrossReferenceTestOne2OneEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_one2oneent2root", typeof(CrossReferenceTestRootEntity), new string[] { "idCol" }
           , new string[] { "idCol" },ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict
           , DeleteRule = ReferentialRuleType.Cascade)]
        public CrossReferenceTestRootEntity RootEntity { get; set; }
    }
}
