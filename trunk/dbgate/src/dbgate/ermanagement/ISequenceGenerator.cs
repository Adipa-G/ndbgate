using System.Data;

namespace dbgate.ermanagement
{
    public interface ISequenceGenerator
    {
        object GetNextSequenceValue(IDbConnection con);
    }
}