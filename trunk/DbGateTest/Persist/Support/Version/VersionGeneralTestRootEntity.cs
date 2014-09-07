using System.Collections.Generic;

namespace DbGate.Persist.Support.Version
{
    [TableInfo("version_test_root")]
    public class VersionGeneralTestRootEntity : DefaultEntity
    {
        public VersionGeneralTestRootEntity()
        {
            One2ManyEntities = new List<VersionGeneralTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Integer)]
        public int Version { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (VersionGeneralTestOne2ManyEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<VersionGeneralTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (VersionGeneralTestOne2OneEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public VersionGeneralTestOne2OneEntity One2OneEntity { get; set; }
    }
}