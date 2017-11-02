using DbGate.ErManagement.DbAbstractionLayer.DataManipulate;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.SqlServerDm;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.SqlServerMm;

namespace DbGate.ErManagement.DbAbstractionLayer
{
    public class SqlServerDbLayer : DefaultDbLayer
    {
        public SqlServerDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IDataManipulate CreateDataManipulate()
        {
            return new SqlServerDataManipulate(this);
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
            return new SqlServerMetaManipulate(this, Config);
        }
    }
}