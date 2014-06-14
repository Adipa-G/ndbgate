using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.exceptions.common
{
    public class ReadFromResultSetException : DbGateException
    {
        public ReadFromResultSetException()
        {
        }

        public ReadFromResultSetException(string message) : base(message)
        {
        }

        public ReadFromResultSetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReadFromResultSetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
