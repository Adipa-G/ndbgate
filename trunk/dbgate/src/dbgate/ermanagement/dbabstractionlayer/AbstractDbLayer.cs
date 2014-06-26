using System.Data;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.dbabstractionlayer
{
    public abstract class AbstractDbLayer : IDbLayer
    {
        protected IDbGateConfig Config;
        private IDataManipulate _dataManipulate;
        private IMetaManipulate _metaManipulate;

        protected AbstractDbLayer(IDbGateConfig config)
        {
            Config = config;
        }

        protected abstract IDataManipulate CreateDataManipulate();

        protected abstract IMetaManipulate CreateMetaManipulate();

        public IDataManipulate DataManipulate()
        {
            if (_dataManipulate == null)
            {
                _dataManipulate = CreateDataManipulate();
            }
            return _dataManipulate;
        }

        public IMetaManipulate MetaManipulate(IDbConnection con)
        {
            if (_metaManipulate == null)
            {
                _metaManipulate = CreateMetaManipulate();
                _metaManipulate.Initialize(con);
            }
            return _metaManipulate;
        }
    }
}
