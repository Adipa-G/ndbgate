using System.Data;
using dbgate.context;

namespace dbgate
{
    public interface IReadOnlyEntity : IReadOnlyClientEntity
    {
        void Retrieve(IDataReader reader, IDbConnection con);

        IEntityContext Context { get; }
    }
}