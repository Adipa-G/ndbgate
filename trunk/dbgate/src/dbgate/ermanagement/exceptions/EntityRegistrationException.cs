
using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class EntityRegistrationException  : BaseException
    {
        public EntityRegistrationException ()
        {
        }

        public EntityRegistrationException (string message) : base(message)
        {
        }

        public EntityRegistrationException (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityRegistrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
