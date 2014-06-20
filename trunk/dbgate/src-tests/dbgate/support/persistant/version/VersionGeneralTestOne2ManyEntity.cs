using dbgate.ermanagement;

namespace dbgate.support.persistant.version
{
    [TableInfo("version_test_one2many")]
    public class VersionGeneralTestOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Integer))]
        public int Version { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }
    }
}

