using dbgate.ermanagement.dbabstractionlayer.datamanipulate;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate.dbdm.accessdm;

namespace dbgate.ermanagement.dbabstractionlayer
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
