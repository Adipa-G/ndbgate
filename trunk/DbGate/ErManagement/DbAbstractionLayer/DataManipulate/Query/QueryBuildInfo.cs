using System;
using System.Collections.Generic;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query
{
    public class QueryBuildInfo
    {
        private readonly Dictionary<string, object> _aliases;
        private readonly QueryExecInfo _execInfo;
        private string _currentQueryId;

        public QueryBuildInfo(QueryBuildInfo queryBuildInfo)
        {
            if (queryBuildInfo != null)
            {
                _execInfo = queryBuildInfo.ExecInfo;
                _aliases = queryBuildInfo.Aliases;
            }
            else
            {
                _execInfo = new QueryExecInfo();
                _aliases = new Dictionary<string, object>();
            }
        }

        public string CurrentQueryId
        {
            set { _currentQueryId = value; }
        }

        public QueryExecInfo ExecInfo
        {
            get { return _execInfo; }
        }

        public Dictionary<string, object> Aliases
        {
            get { return _aliases; }
        }

        public void AddTypeAlias(string alias, Type entityType)
        {
            _aliases.Add(_currentQueryId + alias, entityType);
        }

        public void AddQueryAlias(string alias, ISelectionQuery query)
        {
            _aliases.Add(_currentQueryId + alias, query);
        }

        public void AddUnionAlias(String alias)
        {
            _aliases.Add(_currentQueryId + alias, "UNION");
        }

        public string GetAlias(Object value)
        {
            Dictionary<string, object>.KeyCollection keys = Aliases.Keys;
            foreach (String key in keys)
            {
                if (Aliases[key] == value
                    && key.StartsWith(_currentQueryId))
                {
                    return key.Replace(_currentQueryId, "");
                }
            }
            return null;
        }
    }
}