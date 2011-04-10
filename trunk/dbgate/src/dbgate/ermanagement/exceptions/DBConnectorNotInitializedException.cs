using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class DBConnectorNotInitializedException : BaseException
    {
        public DBConnectorNotInitializedException()
        {
        }

        public DBConnectorNotInitializedException(string message) : base(message)
        {
        }

        public DBConnectorNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DBConnectorNotInitializedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
