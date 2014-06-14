using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.exceptions.common
{
    public class StatementExecutionException : DbGateException
    {
        public StatementExecutionException()
        {
        }

        public StatementExecutionException(string message) : base(message)
        {
        }

        public StatementExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StatementExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
