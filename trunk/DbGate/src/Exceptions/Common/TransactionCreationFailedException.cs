using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class TransactionCreationFailedException   : DbGateException
    {
        public TransactionCreationFailedException  ()
        {
        }

        public TransactionCreationFailedException  (string message) : base(message)
        {
        }

        public TransactionCreationFailedException  (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionCreationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}