using System.Data;

namespace dbgate.ermanagement.support.persistant.columntest
{
    public class PrimaryKeyGenerator : ISequenceGenerator
    {
        public object GetNextSequenceValue(IDbConnection con)
        {
            return 35;
        }
    }
}
