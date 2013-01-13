using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public class QueryExecParam
    {
        public int Index { get; set; }

        public DbType Type { get; set; }

        public object Value { get; set; }
    }
}
