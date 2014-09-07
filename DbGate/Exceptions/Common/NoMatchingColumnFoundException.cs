using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Common
{
    public class NoMatchingColumnFoundException : DbGateException
    {
        public NoMatchingColumnFoundException(string message) : base(message)
        {
        }
    }
}