using System;
using System.Collections.Generic;

namespace dbgate.ermanagement
{
    public interface IManagedRoDbClass : IServerRoDbClass
    {
        Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}