﻿using System;

namespace DbGate.ErManagement.Session
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

        public void Save(IClientEntity clientEntity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}