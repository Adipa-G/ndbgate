using System;
using System.Collections.Generic;

namespace DbGate
{
    public abstract class AbstractManagedReadOnlyEntity : DefaultEntity, IManagedReadOnlyEntity
    {
        #region IManagedReadOnlyEntity Members

        public abstract Dictionary<Type, ICollection<IField>> FieldInfo { get; }

        #endregion
    }
}