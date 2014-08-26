using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [WikiCodeBlock("inheritance_example_top_entity")]
    [TableInfo("top_entity")]
    public class TopEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string SuperName { get; set; }
    }
}