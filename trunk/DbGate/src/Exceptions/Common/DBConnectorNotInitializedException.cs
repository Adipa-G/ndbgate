using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class DbConnectorNotInitializedException : DbGateException
    {
        public DbConnectorNotInitializedException()
        {
        }

        public DbConnectorNotInitializedException(string message) : base(message)
        {
        }

        public DbConnectorNotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DbConnectorNotInitializedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}