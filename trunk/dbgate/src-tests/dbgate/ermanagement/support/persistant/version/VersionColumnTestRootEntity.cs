using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.version
{
    [DbTableInfo("version_test_root")]
    public class VersionColumnTestRootEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Version)]
        public int Version { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
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

