using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class ExpressionParsingError : BaseException
    {
        public ExpressionParsingError(string message) : base(message)
        {
        }

        public ExpressionParsingError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
