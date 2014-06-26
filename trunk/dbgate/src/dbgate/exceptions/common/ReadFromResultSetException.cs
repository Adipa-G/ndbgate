using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions.common
{
    public class ReadFromResultSetException : DbGateException
    {
        public ReadFromResultSetException()
        {
        }

        public ReadFromResultSetException(string message) : base(message)
        {
        }

        public ReadFromResultSetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReadFromResultSetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
