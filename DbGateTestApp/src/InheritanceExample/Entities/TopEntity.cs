using DbGate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [TableInfo("top_entity")]
    public class TopEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string SuperName { get; set; }
    }
}