using System;
using System.Collections.Generic;

namespace dbgate
{
    public interface IManagedEntity : IManagedReadOnlyEntity,IEntity
    {
        Dictionary<Type, string> TableNames { get; }
    }
}