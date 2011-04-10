using System.Data;

namespace dbgate
{
    public interface IServerDbClass : IServerRoDbClass, IDbClass
    {
        void Persist(IDbConnection con);
    }
}