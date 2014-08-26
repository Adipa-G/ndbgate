using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.One2ManyExample.Entities
{
    [WikiCodeBlock("one_2_many_example_child_entity_b")]
    [TableInfo("child_entity_b")]
    public class One2ManyChildEntityB : One2ManyChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}