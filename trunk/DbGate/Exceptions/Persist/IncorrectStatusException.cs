using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Persist
{
    public class IncorrectStatusException : DbGateException
    {
        public IncorrectStatusException(string message) : base(message)
        {
        }
    }
}