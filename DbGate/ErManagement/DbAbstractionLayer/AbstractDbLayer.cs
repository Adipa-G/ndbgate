using System.Data;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public abstract class AbstractDbLayer : IDbLayer
    {
        protected IDbGateConfig Config;
        private IDataManipulate dataManipulate;
        private IMetaManipulate metaManipulate;

        protected AbstractDbLayer(IDbGateConfig config)
        {
            Config = config;
        }

        #region IDbLayer Members

        public IDataManipulate DataManipulate()
        {
            if (dataManipulate == null)
            {
                dataManipulate = CreateDataManipulate();
            }
            return dataManipulate;
        }

        public IMetaManipulate MetaManipulate(ITransaction tx)
        {
            if (metaManipulate == null)
            {
                metaManipulate = CreateMetaManipulate();
                metaManipulate.Initialize(tx);
            }
            return metaManipulate;
        }

        #endregion

        protected abstract IDataManipulate CreateDataManipulate();

        protected abstract IMetaManipulate CreateMetaManipulate();
    }
}