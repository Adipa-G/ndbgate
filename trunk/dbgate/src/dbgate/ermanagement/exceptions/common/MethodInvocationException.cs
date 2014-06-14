using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.exceptions.common
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
