namespace DbGate.Support.Persistant.Version
{
    [TableInfo("version_test_one2one")]
    public class VersionColumnTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Version)]
        public int Version { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}