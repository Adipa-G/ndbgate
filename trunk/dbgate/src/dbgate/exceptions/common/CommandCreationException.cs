using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions.common
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
