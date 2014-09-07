using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Retrival
{
    public class NoSetterFoundToSetChildObjectListException : DbGateException
    {
        public NoSetterFoundToSetChildObjectListException(string message) : base(message)
        {
        }
    }
}