using System.Collections.Generic;

namespace DbGate.Persist.Support.Constraint
{
    [TableInfo("constraint_test_root")]
    public class ConstraintTestReverseRootEntity : DefaultEntity
    {
        public ConstraintTestReverseRootEntity()
        {
            One2ManyEntities = new List<ConstraintTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",
            typeof (ConstraintTestOne2ManyEntity),
            new[] {"idCol"},
            new[] {"idCol"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Restrict)]
        public ICollection<ConstraintTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",
            typeof (ConstraintTestOne2OneEntity),
            new[] {"idCol"},
            new[] {"idCol"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Restrict)]
        public ConstraintTestOne2OneEntity One2OneEntity { get; set; }
    }
}