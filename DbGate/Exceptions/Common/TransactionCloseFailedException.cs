using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCloseFailedException    : DbGateException
    {
        public TransactionCloseFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}