namespace dbgate.ermanagement.impl
{
    public class ErLayerConfig : IErLayerConfig
    {
        public ErLayerConfig()
        {
            AutoTrackChanges = true;
            ShowQueries = true;
            CheckVersion = true;
        }

        public bool AutoTrackChanges { get; set; }

        public string LoggerName { get; set; }

        public bool ShowQueries { get; set; }

        public bool CheckVersion { get; set; }
    }
}
