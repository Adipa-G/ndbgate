using System.Data;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.dbabstractionlayer
{
    public interface IDbLayer
    {
        IDataManipulate DataManipulate();

        IMetaManipulate MetaManipulate(IDbConnection con);
    }
}