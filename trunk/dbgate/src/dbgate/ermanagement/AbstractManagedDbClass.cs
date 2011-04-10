using System;
using System.Collections.Generic;
using System.Data;
using log4net;

namespace dbgate.ermanagement
{
    public abstract class AbstractManagedDbClass : AbstractManagedRoDbClass, IManagedDbClass
    {
        protected AbstractManagedDbClass()
        {
            Status = DbClassStatus.New;
        }

        public abstract Dictionary<Type, string> TableNames { get; }
    }
}