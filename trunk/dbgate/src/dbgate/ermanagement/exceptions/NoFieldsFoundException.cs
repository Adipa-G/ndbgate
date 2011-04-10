
using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class NoFieldsFoundException : BaseException
    {
        public NoFieldsFoundException()
        {
        }

        public NoFieldsFoundException(string message) : base(message)
        {
        }

        public NoFieldsFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoFieldsFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
