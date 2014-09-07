using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class ReadFromResultSetException : DbGateException
    {
        public ReadFromResultSetException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}