﻿using System.Collections.Generic;

namespace DbGate.ErManagement.Session
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

        public Dictionary<string, string> HistoryMappings => historyMappings;
    }
}