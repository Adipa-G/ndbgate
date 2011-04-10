using System;
using System.Collections.Generic;

namespace dbgate.ermanagement
{
    public interface IManagedDbClass : IManagedRoDbClass,IServerDbClass
    {
        Dictionary<Type, string> TableNames { get; }
    }
}