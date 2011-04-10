
using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class IncorrectFieldDefinitionException : BaseException
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
