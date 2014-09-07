using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class DBPatchingException : DbGateException
    {
        public DBPatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}