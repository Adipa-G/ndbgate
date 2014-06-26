using System;
using System.Runtime.Serialization;

namespace dbgate.exceptions
{
    public class DBPatchingException : DbGateException
    {
        public DBPatchingException()
        {
        }

        public DBPatchingException(string message) : base(message)
        {
        }

        public DBPatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DBPatchingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
