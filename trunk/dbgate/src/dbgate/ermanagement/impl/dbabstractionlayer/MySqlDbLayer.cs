using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.mysqlmm;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class MySqlDbLayer  : DefaultDbLayer
    {
        public MySqlDbLayer(IErLayerConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
			return new MySqlMetaManipulate(this,Config);
        }
    }
}
