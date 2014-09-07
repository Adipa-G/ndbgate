using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class StatementExecutionException : DbGateException
    {
        public StatementExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}