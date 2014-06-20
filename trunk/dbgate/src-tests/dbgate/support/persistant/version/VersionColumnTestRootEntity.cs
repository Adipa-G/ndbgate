using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.version
{
    [TableInfo("version_test_root")]
    public class VersionColumnTestRootEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Version)]
        public int Version { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",typeof(VersionColumnTestOne2ManyEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<VersionColumnTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",typeof(VersionColumnTestOne2OneEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade)]
        public VersionColumnTestOne2OneEntity One2OneEntity { get; set; }

        public VersionColumnTestRootEntity()
        {
            One2ManyEntities = new List<VersionColumnTestOne2ManyEntity>();
        }
    }
}

