using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions.common
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
