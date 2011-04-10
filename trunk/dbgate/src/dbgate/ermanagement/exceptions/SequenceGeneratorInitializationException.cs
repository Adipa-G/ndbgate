using System;
using System.Runtime.Serialization;

namespace dbgate.ermanagement.exceptions
{
    public class SequenceGeneratorInitializationException : BaseException
    {
        public SequenceGeneratorInitializationException()
        {
        }

        public SequenceGeneratorInitializationException(string message) : base(message)
        {
        }

        public SequenceGeneratorInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SequenceGeneratorInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
