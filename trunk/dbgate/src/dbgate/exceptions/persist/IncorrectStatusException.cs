using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions.persist
{
    public class IncorrectStatusException : DbGateException
    {
        public IncorrectStatusException()
        {
        }

        public IncorrectStatusException(string message) : base(message)
        {
        }

        public IncorrectStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IncorrectStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
