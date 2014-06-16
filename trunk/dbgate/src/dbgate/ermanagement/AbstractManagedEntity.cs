using System;
using System.Collections.Generic;
using System.Data;
using log4net;

namespace dbgate.ermanagement
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