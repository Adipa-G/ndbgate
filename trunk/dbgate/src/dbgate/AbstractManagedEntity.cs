using System;
using System.Collections.Generic;

namespace dbgate
{
    public abstract class AbstractManagedEntity : AbstractManagedReadOnlyEntity, IManagedEntity
    {
        protected AbstractManagedEntity()
        {
            Status = EntityStatus.New;
        }

        public abstract Dictionary<Type, string> TableNames { get; }
    }
}