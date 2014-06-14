using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions.persist
{
    public class DataUpdatedFromAnotherSourceException : DbGateException
    {
        public DataUpdatedFromAnotherSourceException()
        {
        }

        public DataUpdatedFromAnotherSourceException(string message) : base(message)
        {
        }

        public DataUpdatedFromAnotherSourceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataUpdatedFromAnotherSourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
