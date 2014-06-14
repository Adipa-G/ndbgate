using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions.retrival
{
    public class NoMatchingRecordFoundForSuperClassException : DbGateException
    {
        public NoMatchingRecordFoundForSuperClassException()
        {
        }

        public NoMatchingRecordFoundForSuperClassException(string message) : base(message)
        {
        }

        public NoMatchingRecordFoundForSuperClassException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoMatchingRecordFoundForSuperClassException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}