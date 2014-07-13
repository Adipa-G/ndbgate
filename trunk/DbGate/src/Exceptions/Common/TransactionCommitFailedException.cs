using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCommitFailedException : DbGateException
    {
        public TransactionCommitFailedException  ()
        {
        }

        public TransactionCommitFailedException  (string message) : base(message)
        {
        }

        public TransactionCommitFailedException  (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionCommitFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}