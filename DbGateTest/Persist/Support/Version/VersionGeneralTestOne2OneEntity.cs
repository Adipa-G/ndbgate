namespace DbGate.Persist.Support.Version
{
    [TableInfo("version_test_one2one")]
    public class VersionGeneralTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Version)]
        public int Version { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}