using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.AccessDm;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public class AccessDbLayer : DefaultDbLayer
    {
        public AccessDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IDataManipulate CreateDataManipulate()
        {
            return new AccessDataManipulate(this);
        }
    }
}