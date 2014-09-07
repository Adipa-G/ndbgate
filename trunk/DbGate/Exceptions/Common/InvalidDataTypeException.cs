using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class InvalidDataTypeException : DbGateException
    {
        public InvalidDataTypeException(string message) : base(message)
        {
        }
    }
}