using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class ExpressionParsingException : DbGateException
    {
        public ExpressionParsingException(string message) : base(message)
        {
        }
    }
}