using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Persist
{
    public class DataUpdatedFromAnotherSourceException : DbGateException
    {
        public DataUpdatedFromAnotherSourceException(string message) : base(message)
        {
        }
    }
}