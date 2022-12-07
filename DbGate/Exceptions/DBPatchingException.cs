using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions
{
    public class DbPatchingException : DbGateException
    {
        public DbPatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}