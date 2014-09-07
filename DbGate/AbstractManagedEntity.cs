using System;
using System.Collections.Generic;

namespace DbGate
{
    public abstract class AbstractManagedEntity : AbstractManagedReadOnlyEntity, IManagedEntity
    {
        protected AbstractManagedEntity()
        {
            Status = EntityStatus.New;
        }

        public abstract Dictionary<Type, ITable> TableInfo { get; }
    }
}