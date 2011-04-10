using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class DefaultDbLayer : AbstractDbLayer
    {
        public DefaultDbLayer(IErLayerConfig config) : base(config)
        {
        }

        protected override IDataManipulate CreateDataManipulate()
        {
            return new DefaultDataManipulate(this);
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
            return new DefaultMetaManipulate(this, Config);
        }
    }
}
