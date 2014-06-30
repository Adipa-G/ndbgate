using DbGate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [TableInfo("middle_entity")]
    public class MiddleEntity : TopEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string MiddleName { get; set; }
    }
}