using System.Data;

namespace DbGate.Support.Persistant.ColumnTest
{
    public class PrimaryKeyGenerator : ISequenceGenerator
    {
        #region ISequenceGenerator Members

        public object GetNextSequenceValue(IDbConnection con)
        {
            return 35;
        }

        #endregion
    }
}