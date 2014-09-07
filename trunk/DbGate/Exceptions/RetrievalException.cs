using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class RetrievalException : DbGateException
    {
        public RetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}