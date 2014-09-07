using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Retrival
{
    public class NoMatchingRecordFoundForSuperClassException : DbGateException
    {
        public NoMatchingRecordFoundForSuperClassException(string message) : base(message)
        {
        }
    }
}