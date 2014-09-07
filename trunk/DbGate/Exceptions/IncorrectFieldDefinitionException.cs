using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class IncorrectFieldDefinitionException : DbGateException
    {
        public IncorrectFieldDefinitionException(string message) : base(message)
        {
        }
    }
}