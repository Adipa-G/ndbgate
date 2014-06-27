using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class EntityRegistrationException : DbGateException
    {
        public EntityRegistrationException()
        {
        }

        public EntityRegistrationException(string message) : base(message)
        {
        }

        public EntityRegistrationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityRegistrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}