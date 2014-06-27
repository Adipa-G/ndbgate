using System.Collections.Generic;

namespace DbGate.Support.Persistant.ChangeTracker
{
    [TableInfo("change_tracker_test_root")]
    public class ChangeTrackerTestRootEntity : DefaultEntity
    {
        public ChangeTrackerTestRootEntity()
        {
            One2ManyEntities = new List<ChangeTrackerTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (ChangeTrackerTestOne2ManyEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ChangeTrackerTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (ChangeTrackerTestOne2OneEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ChangeTrackerTestOne2OneEntity One2OneEntity { get; set; }
    }
}