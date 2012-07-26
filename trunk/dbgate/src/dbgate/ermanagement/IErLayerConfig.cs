namespace dbgate.ermanagement
{
    public interface IErLayerConfig
    {
        bool AutoTrackChanges { get; set; }

        string LoggerName { get; set; }

        bool ShowQueries { get; set; }

        bool CheckVersion { get; set; }

        bool EnableStatistics { get; set; }

        bool UpdateChangedColumnsOnly { get; set; }
    }
}
