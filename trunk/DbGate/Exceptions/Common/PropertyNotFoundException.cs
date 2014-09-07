using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class PropertyNotFoundException : DbGateException
    {
        public PropertyNotFoundException(string message) : base(message)
        {
        }
    }
}