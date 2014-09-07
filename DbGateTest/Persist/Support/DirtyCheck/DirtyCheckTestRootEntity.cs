using System.Collections.Generic;

namespace DbGate.Persist.Support.DirtyCheck
{
    [TableInfo("dirty_check_test_root")]
    public class DirtyCheckTestRootEntity : DefaultEntity
    {
        public DirtyCheckTestRootEntity()
        {
            One2ManyEntities = new List<DirtyCheckTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (DirtyCheckTestOne2ManyEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<DirtyCheckTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (DirtyCheckTestOne2OneEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public DirtyCheckTestOne2OneEntity One2OneEntity { get; set; }
    }
}