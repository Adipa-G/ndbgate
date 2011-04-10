using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.crossreference
{
    [DbTableInfo("cross_reference_test_root")]
    public class CrossReferenceTestRootEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",typeof(CrossReferenceTestOne2ManyEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<CrossReferenceTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof(CrossReferenceTestOne2OneEntity), new string[] { "idCol" }
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public CrossReferenceTestOne2OneEntity One2OneEntity { get; set; }

        public CrossReferenceTestRootEntity()
        {
            One2ManyEntities = new List<CrossReferenceTestOne2ManyEntity>();
        }
    }
}