using DbGate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [TableInfo("bottom_entity")]
    public class BottomEntity : MiddleEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string SubName { get; set; }
    }
}