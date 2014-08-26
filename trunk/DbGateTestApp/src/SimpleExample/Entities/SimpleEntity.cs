using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.SimpleExample.Entities
{
    [WikiCodeBlock("simple_example_simple_entity")]
    [TableInfo("simple_entity")]
    public class SimpleEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}