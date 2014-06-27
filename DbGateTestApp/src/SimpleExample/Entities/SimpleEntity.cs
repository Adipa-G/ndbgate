using DbGate;

namespace DbGateTestApp.SimpleExample.Entities
{
    [TableInfo("simple_entity")]
    public class SimpleEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}