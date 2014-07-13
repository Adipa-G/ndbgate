using System.Data;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public interface IDbLayer
    {
        IDataManipulate DataManipulate();

        IMetaManipulate MetaManipulate(ITransaction tx);
    }
}