using System.Data;

namespace DbGate.Support.Persistant.ColumnTest
{
    public class PrimaryKeyGenerator : ISequenceGenerator
    {
        #region ISequenceGenerator Members

        public object GetNextSequenceValue(ITransaction tx)
        {
            return 35;
        }

        #endregion
    }
}