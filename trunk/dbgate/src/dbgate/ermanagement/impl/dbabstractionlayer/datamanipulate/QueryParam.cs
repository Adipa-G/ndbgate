using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public class QueryParam
    {
        public int Index { get; set; }

        public DbType Type { get; set; }

        public object Value { get; set; }
    }
}
