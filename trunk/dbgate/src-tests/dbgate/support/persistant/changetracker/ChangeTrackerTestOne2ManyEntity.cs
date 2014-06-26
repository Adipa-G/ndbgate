using dbgate.ermanagement;

namespace dbgate.support.persistant.changetracker
{
    [TableInfo("change_tracker_test_one2many")]
    public class ChangeTrackerTestOne2ManyEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }
    }
}