namespace DbGate.Support.Persistant.ChangeTracker
{
    [TableInfo("change_tracker_test_one2one")]
    public class ChangeTrackerTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}