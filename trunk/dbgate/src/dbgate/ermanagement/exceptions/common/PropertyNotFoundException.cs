using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.exceptions.common
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
