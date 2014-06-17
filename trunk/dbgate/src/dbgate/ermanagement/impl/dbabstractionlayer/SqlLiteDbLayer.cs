using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.sqllitemm;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class SqlLiteDbLayer  : DefaultDbLayer
    {
        public SqlLiteDbLayer(IDbGateConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
			return new SqlLiteMetaManipulate(this,Config);
        }
    }
}
