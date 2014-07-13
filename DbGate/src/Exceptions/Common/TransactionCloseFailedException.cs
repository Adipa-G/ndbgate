using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCloseFailedException    : DbGateException
    {
        public TransactionCloseFailedException()
        {
        }

        public TransactionCloseFailedException(string message) : base(message)
        {
        }

        public TransactionCloseFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionCloseFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}