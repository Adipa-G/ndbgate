using DbGate;

namespace DbGateTestApp.InheritanceExample.Entities
{
    [TableInfo("super_entity")]
    public class SuperEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int Id { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string SuperName { get; set; }
    }
}