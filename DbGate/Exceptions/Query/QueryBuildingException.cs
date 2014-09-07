using System;
using System.Runtime.Serialization;

namespace DbGate.Exceptions.Query
{
    public class QueryBuildingException : DbGateException
    {
        public QueryBuildingException(string message) : base(message)
        {
        }
    }
}