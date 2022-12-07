using System;
using System.Collections.Generic;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query
{
    public class QueryBuildInfo
    {
        private readonly Dictionary<string, object> aliases;
        private readonly QueryExecInfo execInfo;
        private string currentQueryId;

        public QueryBuildInfo(QueryBuildInfo queryBuildInfo)
        {
            if (queryBuildInfo != null)
            {
                execInfo = queryBuildInfo.ExecInfo;
                aliases = queryBuildInfo.Aliases;
            }
            else
            {
                execInfo = new QueryExecInfo();
                aliases = new Dictionary<string, object>();
            }
        }

        public string CurrentQueryId
        {
            set => currentQueryId = value;
        }

        public QueryExecInfo ExecInfo => execInfo;

        public Dictionary<string, object> Aliases => aliases;

        public void AddTypeAlias(string alias, Type entityType)
        {
            aliases.Add(currentQueryId + alias, entityType);
        }

        public void AddQueryAlias(string alias, ISelectionQuery query)
        {
            aliases.Add(currentQueryId + alias, query);
        }

        public void AddUnionAlias(String alias)
        {
            aliases.Add(currentQueryId + alias, "UNION");
        }

        public string GetAlias(Object value)
        {
            var keys = Aliases.Keys;
            foreach (var key in keys)
            {
                if (Aliases[key] == value
                    && key.StartsWith(currentQueryId))
                {
                    return key.Replace(currentQueryId, "");
                }
            }
            return null;
        }
    }
}