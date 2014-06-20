using System.Data;

namespace dbgate
{
    public interface ISequenceGenerator
    {
        object GetNextSequenceValue(IDbConnection con);
    }
}