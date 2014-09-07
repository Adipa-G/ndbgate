using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [WikiCodeBlock("inheritance_example_bottom_entity")]
    [TableInfo("bottom_entity")]
    public class BottomEntity : MiddleEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string SubName { get; set; }
    }
}