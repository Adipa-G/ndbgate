using System;
using System.Collections.Generic;

namespace dbgate
{
    public abstract class AbstractManagedReadOnlyEntity : DefaultEntity, IManagedReadOnlyEntity
    {
        public abstract Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}