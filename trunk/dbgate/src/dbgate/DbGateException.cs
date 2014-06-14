using System;
using System.Runtime.Serialization;

namespace dbgate
{
    public class DbGateException : Exception
    {
        public DbGateException()
        {
        }

        public DbGateException(string message) : base(message)
        {
        }

        public DbGateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DbGateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
