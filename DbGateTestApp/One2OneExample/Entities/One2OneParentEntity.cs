using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2OneExample.Entities
{
    [WikiCodeBlock("one_2_one_example_parent_entity")]
    [TableInfo("parent_entity")]
    public class One2OneParentEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("parent2childA",
            typeof (One2OneChildEntityA),
            new[] {"id"},
            new[] {"parentId"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("parent2childB",
            typeof (One2OneChildEntityB),
            new[] {"id"},
            new[] {"parentId"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public One2OneChildEntity ChildEntity { get; set; }
    }
}