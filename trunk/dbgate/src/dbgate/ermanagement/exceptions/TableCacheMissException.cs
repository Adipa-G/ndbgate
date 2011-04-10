using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class TableCacheMissException : BaseException
    {
        public TableCacheMissException()
        {
        }

        public TableCacheMissException(string message) : base(message)
        {
        }

        public TableCacheMissException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TableCacheMissException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
