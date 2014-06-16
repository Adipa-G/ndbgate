using System.Data;

namespace dbgate
{
    public interface IEntity : IReadOnlyEntity, IClientEntity
    {
        void Persist(IDbConnection con);
    }
}