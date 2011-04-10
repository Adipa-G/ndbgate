namespace dbgate.ermanagement.support.persistant.version
{
    [DbTableInfo("version_test_one2one")]
    public class VersionGeneralTestOne2OneEntity : DefaultServerDbClass
    {
        public VersionGeneralTestOne2OneEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo(DbColumnType.Version)]
        public int Version { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }
    }
}
