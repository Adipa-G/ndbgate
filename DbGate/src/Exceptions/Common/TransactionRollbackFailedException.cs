using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionRollbackFailedException  : DbGateException
    {
        public TransactionRollbackFailedException ()
        {
        }

        public TransactionRollbackFailedException (string message) : base(message)
        {
        }

        public TransactionRollbackFailedException (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionRollbackFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}