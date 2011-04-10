using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.constraint
{
    [DbTableInfo("constraint_test_root")]
    public class ConstraintTestDeleteRestrictRootEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof(ConstraintTestOne2ManyEntity), new string[] { "idCol" }
            , new string[] { "idCol" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Restrict)]
        public ICollection<ConstraintTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof(ConstraintTestOne2OneEntity), new string[] { "idCol" }
            , new string[] { "idCol" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Restrict)]
        public ConstraintTestOne2OneEntity One2OneEntity { get; set; }

        public ConstraintTestDeleteRestrictRootEntity()
        {
            One2ManyEntities = new List<ConstraintTestOne2ManyEntity>();
        }
    }
}