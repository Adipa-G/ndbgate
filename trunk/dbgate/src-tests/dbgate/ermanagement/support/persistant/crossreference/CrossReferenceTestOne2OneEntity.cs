namespace dbgate.ermanagement.support.persistant.crossreference
{
    [DbTableInfo("cross_reference_test_one2one")]
    public class CrossReferenceTestOne2OneEntity : DefaultServerDbClass
    {
        public CrossReferenceTestOne2OneEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_one2oneent2root", typeof(CrossReferenceTestRootEntity), new string[] { "idCol" }
           , new string[] { "idCol" },ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict
           , DeleteRule = ReferentialRuleType.Cascade)]
        public CrossReferenceTestRootEntity RootEntity { get; set; }
    }
}
