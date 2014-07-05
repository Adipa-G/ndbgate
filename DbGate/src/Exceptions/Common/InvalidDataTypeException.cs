using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class InvalidDataTypeException : DbGateException
    {
        public InvalidDataTypeException()
        {
        }

        public InvalidDataTypeException(string message) : base(message)
        {
        }

        public InvalidDataTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidDataTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}