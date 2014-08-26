using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2OneExample.Entities
{
    [WikiCodeBlock("one_2_one_example_child_entity_a")]
    [TableInfo("child_entity_a")]
    public class One2OneChildEntityA : One2OneChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}