using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.exceptions.common
{
    public class CommandCreationException : DbGateException
    {
        public CommandCreationException()
        {
        }

        public CommandCreationException(string message) : base(message)
        {
        }

        public CommandCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CommandCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
