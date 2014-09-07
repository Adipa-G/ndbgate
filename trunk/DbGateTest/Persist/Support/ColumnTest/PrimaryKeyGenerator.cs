namespace DbGate.Persist.Support.ColumnTest
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