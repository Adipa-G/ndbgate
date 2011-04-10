using System;
using System.Collections.Generic;
using System.Data;
using log4net;

namespace dbgate.ermanagement
{
    public abstract class AbstractManagedRoDbClass : DefaultServerDbClass, IManagedRoDbClass
    {
        public abstract Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}