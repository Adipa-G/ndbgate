using dbgate.ermanagement.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.dbmm.sqllitemm;

namespace dbgate.ermanagement.dbabstractionlayer
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
