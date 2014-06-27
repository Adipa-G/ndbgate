using DbGate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [TableInfo("sub_entity")]
    public class SubEntity : MiddleEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string SubName { get; set; }
    }
}