using System.Collections.Generic;

namespace dbgate.ermanagement.caches.impl
{
    public class QueryHolder
    {
        private readonly Dictionary<string, string> _queryMap;

        public QueryHolder()
        {
            _queryMap = new Dictionary<string, string>();
        }

        public void SetQuery(string id, string query)
        {
            if (_queryMap.ContainsKey(id))
            {
                _queryMap.Remove(id);
            }
            _queryMap.Add(id, query);
        }

        public string GetQuery(string id)
        {
            if (_queryMap.ContainsKey(id))
            {
                return _queryMap[id];
            }
            return null;
        }
    }
}