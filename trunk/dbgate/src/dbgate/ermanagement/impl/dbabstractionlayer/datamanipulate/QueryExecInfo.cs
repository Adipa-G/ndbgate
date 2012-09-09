using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public class QueryExecInfo
    {
        private ICollection<QueryParam> _params; 

        public QueryExecInfo()
        {
            _params = new Collection<QueryParam>();
        }

        public string Sql { get; set; }

        public ICollection<QueryParam> Params
        {
            get { return _params; }
        }
    }
}
