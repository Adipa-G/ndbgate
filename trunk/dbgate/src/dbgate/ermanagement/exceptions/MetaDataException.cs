using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class MetaDataException : BaseException
    {
        public MetaDataException()
        {
        }

        public MetaDataException(string message) : base(message)
        {
        }

        public MetaDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MetaDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
