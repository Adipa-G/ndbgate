
using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class FieldCacheMissException : BaseException
    {
        public FieldCacheMissException()
        {
        }

        public FieldCacheMissException(string message) : base(message)
        {
        }

        public FieldCacheMissException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FieldCacheMissException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
