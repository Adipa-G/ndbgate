using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Persist
{
    public class IntegrityConstraintViolationException : DbGateException
    {
        public IntegrityConstraintViolationException(string message) : base(message)
        {
        }
    }
}