using System.Collections.Generic;
using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2ManyExample.Entities
{
    [WikiCodeBlock("one_2_many_example_parent_entity")]
    [TableInfo("parent_entity")]
    public class One2ManyParentEntity : DefaultEntity
    {
        public One2ManyParentEntity()
        {
            ChildEntities = new List<One2ManyChildEntity>();
        }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("parent2childA", typeof (One2ManyChildEntityA), new[] {"id"}
            , new[] {"parentId"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("parent2childB", typeof (One2ManyChildEntityB), new[] {"id"}
            , new[] {"parentId"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<One2ManyChildEntity> ChildEntities { get; set; }
    }
}