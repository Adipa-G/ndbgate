using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class MethodInvocationException : DbGateException
    {
        public MethodInvocationException()
        {
        }

        public MethodInvocationException(string message) : base(message)
        {
        }

        public MethodInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MethodInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}