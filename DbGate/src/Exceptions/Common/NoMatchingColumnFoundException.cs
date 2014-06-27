using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class NoMatchingColumnFoundException : DbGateException
    {
        public NoMatchingColumnFoundException()
        {
        }

        public NoMatchingColumnFoundException(string message) : base(message)
        {
        }

        public NoMatchingColumnFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoMatchingColumnFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}