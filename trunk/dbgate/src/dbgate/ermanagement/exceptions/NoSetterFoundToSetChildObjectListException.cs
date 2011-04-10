using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class NoSetterFoundToSetChildObjectListException : BaseException
    {
        public NoSetterFoundToSetChildObjectListException()
        {
        }

        public NoSetterFoundToSetChildObjectListException(string message) : base(message)
        {
        }

        public NoSetterFoundToSetChildObjectListException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoSetterFoundToSetChildObjectListException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
