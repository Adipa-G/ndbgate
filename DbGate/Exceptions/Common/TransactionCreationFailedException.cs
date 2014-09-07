using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCreationFailedException   : DbGateException
    {
        public TransactionCreationFailedException  (string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}