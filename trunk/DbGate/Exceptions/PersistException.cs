using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class PersistException : DbGateException
    {
        public PersistException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}