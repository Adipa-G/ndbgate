using System;

namespace dbgate.ermanagement.session
{
    public class Session : ISession
    {
        #region ISession Members

        public void StartTransaction()
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void Save(IDbClass dbClass)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}