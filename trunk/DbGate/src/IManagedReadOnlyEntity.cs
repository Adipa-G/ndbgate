using System;
using System.Collections.Generic;

namespace DbGate
{
    public interface IManagedReadOnlyEntity : IReadOnlyEntity
    {
        Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}