namespace dbgate.ermanagement.support.persistant.changetracker
{
    [TableInfo("change_tracker_test_one2one")]
    public class ChangeTrackerTestOne2OneEntity : DefaultEntity
    {
        public ChangeTrackerTestOne2OneEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}