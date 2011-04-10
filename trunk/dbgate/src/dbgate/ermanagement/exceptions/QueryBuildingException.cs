using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class QueryBuildingException : BaseException
    {
        public QueryBuildingException()
        {
        }

        public QueryBuildingException(string message) : base(message)
        {
        }

        public QueryBuildingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected QueryBuildingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
