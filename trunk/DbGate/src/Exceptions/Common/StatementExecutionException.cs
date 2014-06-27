using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class StatementExecutionException : DbGateException
    {
        public StatementExecutionException()
        {
        }

        public StatementExecutionException(string message) : base(message)
        {
        }

        public StatementExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StatementExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}