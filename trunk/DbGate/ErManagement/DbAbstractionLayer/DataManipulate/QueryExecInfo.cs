using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate
{
    public class QueryExecInfo
    {
        private readonly ICollection<QueryExecParam> _params;

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