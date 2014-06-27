using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.SqlLiteMm;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public class SqlLiteDbLayer : DefaultDbLayer
    {
        public SqlLiteDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
            return new SqlLiteMetaManipulate(this, Config);
        }
    }
}