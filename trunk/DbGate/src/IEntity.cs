using System.Data;

namespace DbGate
{
    public interface IEntity : IReadOnlyEntity, IClientEntity
    {
        void Persist(IDbConnection con);
    }
}