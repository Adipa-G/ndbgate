using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class RetrievalException : DbGateException
    {
        public RetrievalException()
        {
        }

        public RetrievalException(string message) : base(message)
        {
        }

        public RetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RetrievalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
