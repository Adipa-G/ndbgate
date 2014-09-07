using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class EntityRegistrationException : DbGateException
    {
        public EntityRegistrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}