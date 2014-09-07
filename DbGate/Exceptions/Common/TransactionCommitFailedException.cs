using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCommitFailedException : DbGateException
    {
        public TransactionCommitFailedException  (string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}