using System.Data;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public interface IDbLayer
    {
        IDataManipulate DataManipulate();

        IMetaManipulate MetaManipulate(IDbConnection con);
    }
}