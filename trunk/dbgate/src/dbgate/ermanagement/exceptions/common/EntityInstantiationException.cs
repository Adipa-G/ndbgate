using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions.common
{
    public class EntityInstantiationException  : DbGateException
    {
        public EntityInstantiationException ()
        {
        }

        public EntityInstantiationException (string message) : base(message)
        {
        }

        public EntityInstantiationException (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityInstantiationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
