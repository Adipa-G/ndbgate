using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate
{
    public class QueryExecInfo
    {
        private ICollection<QueryExecParam> _params; 

        public QueryExecInfo()
        {
            _params = new Collection<QueryExecParam>();
        }

        public string Sql { get; set; }

        public ICollection<QueryExecParam> Params
        {
            get { return _params; }
        }
    }
}
