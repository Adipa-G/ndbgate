using System;
using System.Collections.Generic;

namespace dbgate
{
    public interface IManagedReadOnlyEntity : IReadOnlyEntity
    {
        Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}