using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.DefaultDm;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.DefaultMm;

namespace DbGate.ErManagement.DbAbstractionLayer
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
