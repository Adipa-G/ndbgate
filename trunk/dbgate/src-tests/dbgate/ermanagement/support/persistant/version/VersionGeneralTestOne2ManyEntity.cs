namespace dbgate.ermanagement.support.persistant.version
{
    [DbTableInfo("version_test_one2many")]
    public class VersionGeneralTestOne2ManyEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [DbColumnInfo((DbColumnType.Integer))]
        public int Version { get; set; }

        [DbColumnInfo((DbColumnType.Varchar))]
        public string Name { get; set; }
    }
}

