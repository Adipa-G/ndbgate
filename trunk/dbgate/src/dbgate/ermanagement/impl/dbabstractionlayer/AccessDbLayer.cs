using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class AccessDbLayer : DefaultDbLayer
    {
        public AccessDbLayer(IErLayerConfig config) : base(config)
        {
        }

        protected override IDataManipulate CreateDataManipulate()
        {
            return new AccessDataManipulate(this);
        }
    }
}
