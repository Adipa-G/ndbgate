namespace dbgate.ermanagement.support.persistant.changetracker
{
    [DbTableInfo("change_tracker_test_one2one")]
    public class ChangeTrackerTestOne2OneEntity : DefaultServerDbClass
    {
        public ChangeTrackerTestOne2OneEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }
    }
}