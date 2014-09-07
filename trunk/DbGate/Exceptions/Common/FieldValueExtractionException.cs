using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class FieldValueExtractionException : DbGateException
    {
        public FieldValueExtractionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}