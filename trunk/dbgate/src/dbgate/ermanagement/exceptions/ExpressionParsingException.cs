
using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class ExpressionParsingException  : DbGateException
    {
        public ExpressionParsingException ()
        {
        }

        public ExpressionParsingException (string message) : base(message)
        {
        }

        public ExpressionParsingException (string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExpressionParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
