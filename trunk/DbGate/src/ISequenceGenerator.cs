using System.Data;

namespace DbGate
{
    public interface ISequenceGenerator
    {
        object GetNextSequenceValue(IDbConnection con);
    }
}