using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions.common
{
    public class PropertyNotFoundException : DbGateException
    {
        public PropertyNotFoundException()
        {
        }

        public PropertyNotFoundException(string message) : base(message)
        {
        }

        public PropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
