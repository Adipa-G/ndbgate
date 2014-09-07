using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionRollbackFailedException  : DbGateException
    {
        public TransactionRollbackFailedException (string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}