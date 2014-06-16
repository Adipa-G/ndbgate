namespace dbgate.ermanagement.support.persistant.version
{
    [TableInfo("version_test_one2one")]
    public class VersionGeneralTestOne2OneEntity : DefaultEntity
    {
        public VersionGeneralTestOne2OneEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Version)]
        public int Version { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}
