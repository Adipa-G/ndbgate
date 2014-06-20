using dbgate.ermanagement;

namespace dbgate.support.persistant.version
{
    [TableInfo("version_test_one2one")]
    public class VersionColumnTestOne2OneEntity : DefaultEntity
    {
        public VersionColumnTestOne2OneEntity()
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
