using System;
using System.Collections.Generic;

namespace DbGate
{
    public interface IManagedEntity : IManagedReadOnlyEntity, IEntity
    {
        Dictionary<Type, string> TableNames { get; }
    }
}