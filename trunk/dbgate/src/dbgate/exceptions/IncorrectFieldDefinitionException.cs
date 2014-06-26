using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions
{
    public class IncorrectFieldDefinitionException : DbGateException
    {
        public IncorrectFieldDefinitionException()
        {
        }

        public IncorrectFieldDefinitionException(string message) : base(message)
        {
        }

        public IncorrectFieldDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IncorrectFieldDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
