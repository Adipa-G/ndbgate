namespace DbGate
{
    public interface IDbGateConfig
    {
        bool ShowQueries { get; set; }

        bool EnableStatistics { get; set; }

        string LoggerName { get; set; }

        DirtyCheckStrategy DirtyCheckStrategy { get; set; }
        
        VerifyOnWriteStrategy VerifyOnWriteStrategy { get; set; }

        UpdateStrategy UpdateStrategy { get; set; }

        FetchStrategy FetchStrategy { get; set; }
    }
}