using DbGate.Context;
using System.Data;

namespace DbGate
{
    public interface IReadOnlyEntity : IReadOnlyClientEntity
    {
        IEntityContext Context { get; }
        void Retrieve(IDataReader reader, ITransaction tx);
    }
}