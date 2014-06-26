using dbgate.ermanagement.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.dbmm.myqlmm;

namespace dbgate.ermanagement.dbabstractionlayer
{
    public class MySqlDbLayer  : DefaultDbLayer
    {
        public MySqlDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
			return new MySqlMetaManipulate(this,Config);
        }
    }
}
