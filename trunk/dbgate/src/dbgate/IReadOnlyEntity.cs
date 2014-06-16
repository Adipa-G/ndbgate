using System.Data;
using dbgate.ermanagement.context;

namespace dbgate
{
    public interface IReadOnlyEntity : IReadOnlyClientEntity
    {
        void Retrieve(IDataReader reader, IDbConnection con);

        IEntityContext Context { get; }
    }
}