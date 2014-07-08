using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class FieldValueExtractionException : DbGateException
    {
        public FieldValueExtractionException()
        {
        }

        public FieldValueExtractionException(string message) : base(message)
        {
        }

        public FieldValueExtractionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FieldValueExtractionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}