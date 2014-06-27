using DbGate;

namespace DbGateTestApp.One2OneExample.Entities
{
    [TableInfo("child_entity_b")]
    public class One2OneChildEntityB : One2OneChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}