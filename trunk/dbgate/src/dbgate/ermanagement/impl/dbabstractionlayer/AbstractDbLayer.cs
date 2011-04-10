using System.Data;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public abstract class AbstractDbLayer : IDbLayer
    {
        protected IErLayerConfig Config;
        private IDataManipulate _dataManipulate;
        private IMetaManipulate _metaManipulate;

        protected AbstractDbLayer(IErLayerConfig config)
        {
            Config = config;
        }

        protected abstract IDataManipulate CreateDataManipulate();

        protected abstract IMetaManipulate CreateMetaManipulate();

        public IDataManipulate GetDataManipulate()
        {
            if (_dataManipulate == null)
            {
                _dataManipulate = CreateDataManipulate();
            }
            return _dataManipulate;
        }

        public IMetaManipulate GetMetaManipulate(IDbConnection con)
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
