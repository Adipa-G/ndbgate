using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class MethodInvocationException : DbGateException
    {
        public MethodInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}