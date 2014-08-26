using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2OneExample.Entities
{
    [WikiCodeBlock("one_2_one_example_child_entity_b")]
    [TableInfo("child_entity_b")]
    public class One2OneChildEntityB : One2OneChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}