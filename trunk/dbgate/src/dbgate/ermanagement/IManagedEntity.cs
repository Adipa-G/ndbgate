using System;
using System.Collections.Generic;

namespace dbgate.ermanagement
{
    public interface IManagedEntity : IManagedReadOnlyEntity,IEntity
    {
        Dictionary<Type, string> TableNames { get; }
    }
}