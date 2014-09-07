using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class EntityInstantiationException : DbGateException
    {
        public EntityInstantiationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}