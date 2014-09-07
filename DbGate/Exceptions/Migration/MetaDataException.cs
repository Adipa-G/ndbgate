using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Migration
{
    public class MetaDataException : DbGateException
    {
        public MetaDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}