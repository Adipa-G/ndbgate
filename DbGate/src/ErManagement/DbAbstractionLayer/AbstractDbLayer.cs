using System.Data;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;

namespace DbGate.ErManagement.DbAbstractionLayer
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

        #region IDbLayer Members

        public IDataManipulate DataManipulate()
        {
            if (_dataManipulate == null)
            {
                _dataManipulate = CreateDataManipulate();
            }
            return _dataManipulate;
        }

        public IMetaManipulate MetaManipulate(ITransaction tx)
        {
            if (_metaManipulate == null)
            {
                _metaManipulate = CreateMetaManipulate();
                _metaManipulate.Initialize(tx);
            }
            return _metaManipulate;
        }

        #endregion

        protected abstract IDataManipulate CreateDataManipulate();

        protected abstract IMetaManipulate CreateMetaManipulate();
    }
}