using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class CommandCreationException : DbGateException
    {
        public CommandCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}