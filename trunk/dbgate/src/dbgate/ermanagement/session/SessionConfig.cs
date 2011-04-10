using System.Collections.Generic;

namespace dbgate.ermanagement.session
{
    public class SessionConfig
    {
        private readonly Dictionary<string, string> historyMappings;

        public SessionConfig()
        {
            EnableHistory = false;
            historyMappings = new Dictionary<string, string>();
        }

        public bool EnableHistory { get; set; }

        public Dictionary<string, string> HistoryMappings
        {
            get { return historyMappings; }
        }
    }
}