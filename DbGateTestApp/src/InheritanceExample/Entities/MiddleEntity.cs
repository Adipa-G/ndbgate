using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [WikiCodeBlock("inheritance_example_middle_entity")]
    [TableInfo("middle_entity")]
    public class MiddleEntity : TopEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string MiddleName { get; set; }
    }
}