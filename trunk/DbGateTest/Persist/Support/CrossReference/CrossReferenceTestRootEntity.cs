using System.Collections.Generic;

namespace DbGate.Persist.Support.CrossReference
{
    [TableInfo("cross_reference_test_root")]
    public class CrossReferenceTestRootEntity : DefaultEntity
    {
        public CrossReferenceTestRootEntity()
        {
            One2ManyEntities = new List<CrossReferenceTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (CrossReferenceTestOne2ManyEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<CrossReferenceTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (CrossReferenceTestOne2OneEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public CrossReferenceTestOne2OneEntity One2OneEntity { get; set; }
    }
}