using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.defaultmm;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.dbdm.defaultdm;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class DefaultDbLayer : AbstractDbLayer
    {
        public DefaultDbLayer(IDbGateConfig config) : base(config)
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
