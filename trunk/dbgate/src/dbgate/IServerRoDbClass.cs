using System.Data;
using dbgate.ermanagement.context;

namespace dbgate
{
    public interface IServerRoDbClass : IRoDbClass
    {
        void Retrieve(IDataReader reader, IDbConnection con);

        IEntityContext Context { get; }
    }
}