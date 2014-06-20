using System.Data;
using dbgate.ermanagement;

namespace dbgate.support.persistant.columntest
{
    public class PrimaryKeyGenerator : ISequenceGenerator
    {
        public object GetNextSequenceValue(IDbConnection con)
        {
            return 35;
        }
    }
}
