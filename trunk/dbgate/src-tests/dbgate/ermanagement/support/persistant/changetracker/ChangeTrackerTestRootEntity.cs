using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.changetracker
{
    [DbTableInfo("change_tracker_test_root")]
    public class ChangeTrackerTestRootEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",typeof(ChangeTrackerTestOne2ManyEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ChangeTrackerTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",typeof(ChangeTrackerTestOne2OneEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public ChangeTrackerTestOne2OneEntity One2OneEntity { get; set; }

        public ChangeTrackerTestRootEntity()
        {
            One2ManyEntities = new List<ChangeTrackerTestOne2ManyEntity>();
        }
    }
}
