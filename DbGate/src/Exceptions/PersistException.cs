using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class PersistException : DbGateException
    {
        public PersistException()
        {
        }

        public PersistException(string message) : base(message)
        {
        }

        public PersistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PersistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}