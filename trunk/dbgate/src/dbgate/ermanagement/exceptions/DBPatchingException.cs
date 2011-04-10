using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class DBPatchingException : BaseException
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
