namespace DbGate.ErManagement.ErMapper
{
    public class DbGateConfig : IDbGateConfig
    {
        public DbGateConfig()
        {
            AutoTrackChanges = true;
            ShowQueries = true;
            EnableStatistics = false;

            DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            VerifyOnWriteStrategy = VerifyOnWriteStrategy.Verify;
            UpdateStrategy = UpdateStrategy.ChangedColumns;
            FetchStrategy = FetchStrategy.Eager;
        }

        #region IDbGateConfig Members

        public bool AutoTrackChanges { get; set; }

        public string LoggerName { get; set; }

        public bool ShowQueries { get; set; }

        public bool EnableStatistics { get; set; }

        public DirtyCheckStrategy DirtyCheckStrategy { get; set; }

        public VerifyOnWriteStrategy VerifyOnWriteStrategy { get; set; }

        public UpdateStrategy UpdateStrategy { get; set; }

        public FetchStrategy FetchStrategy { get; set; }

        #endregion
    }
}