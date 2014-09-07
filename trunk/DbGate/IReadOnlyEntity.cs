using System.Data;
using DbGate.Context;

namespace DbGate
{
    public interface IReadOnlyEntity : IReadOnlyClientEntity
    {
        IEntityContext Context { get; }
        void Retrieve(IDataReader reader, ITransaction tx);
    }
}