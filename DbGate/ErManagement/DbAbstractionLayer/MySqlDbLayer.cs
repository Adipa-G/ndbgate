using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.MySqlMm;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public class MySqlDbLayer : DefaultDbLayer
    {
        public MySqlDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
            return new MySqlMetaManipulate(this, Config);
        }
    }
}