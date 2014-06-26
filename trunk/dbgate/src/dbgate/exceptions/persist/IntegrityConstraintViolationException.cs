using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions.persist
{
    public class IntegrityConstraintViolationException : DbGateException
    {
        public IntegrityConstraintViolationException()
        {
        }

        public IntegrityConstraintViolationException(string message) : base(message)
        {
        }

        public IntegrityConstraintViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IntegrityConstraintViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
