using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using System.Data;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public interface IDbLayer
    {
        IDataManipulate DataManipulate();

        IMetaManipulate MetaManipulate(ITransaction tx);
    }
}