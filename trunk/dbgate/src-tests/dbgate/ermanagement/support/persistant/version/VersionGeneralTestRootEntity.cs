using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.version
{
    [DbTableInfo("version_test_root")]
    public class VersionGeneralTestRootEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Integer)]
        public int Version { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof(VersionGeneralTestOne2ManyEntity), new string[] { "idCol" }
            , new string[] { "idCol" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<VersionGeneralTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof(VersionGeneralTestOne2OneEntity), new string[] { "idCol" }
            , new string[] { "idCol" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public VersionGeneralTestOne2OneEntity One2OneEntity { get; set; }

        public VersionGeneralTestRootEntity()
        {
            One2ManyEntities = new List<VersionGeneralTestOne2ManyEntity>();
        }
    }
}

