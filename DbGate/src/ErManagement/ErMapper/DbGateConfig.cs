namespace DbGate.ErManagement.ErMapper
{
    public class DbGateConfig : IDbGateConfig
    {
        public DbGateConfig()
        {
            AutoTrackChanges = true;
            ShowQueries = true;
            CheckVersion = true;
            EnableStatistics = false;
            UpdateChangedColumnsOnly = true;
        }

        #region IDbGateConfig Members

        public bool AutoTrackChanges { get; set; }

        public string LoggerName { get; set; }

        public bool ShowQueries { get; set; }

        public bool CheckVersion { get; set; }

        public bool EnableStatistics { get; set; }

        public bool UpdateChangedColumnsOnly { get; set; }

        #endregion
    }
}