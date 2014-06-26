using dbgate.ermanagement.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate.dbdm.defaultdm;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.dbmm.defaultmm;

namespace dbgate.ermanagement.dbabstractionlayer
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
