using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class SqlLiteDbLayer  : DefaultDbLayer
    {
        public SqlLiteDbLayer(IErLayerConfig config) : base(config)
        {
        }

        protected override IMetaManipulate CreateMetaManipulate()
        {
            return new SqlLiteMetaManipulate(this,Config);
        }
    }
}
