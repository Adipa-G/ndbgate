using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Migration
{
    public class MetaDataException : DbGateException
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