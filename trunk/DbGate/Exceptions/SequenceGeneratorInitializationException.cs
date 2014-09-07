using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class SequenceGeneratorInitializationException : DbGateException
    {
        public SequenceGeneratorInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}