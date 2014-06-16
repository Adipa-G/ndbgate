using System;
using System.Collections.Generic;
using System.Data;
using log4net;

namespace dbgate.ermanagement
{
    public abstract class AbstractManagedReadOnlyEntity : DefaultEntity, IManagedReadOnlyEntity
    {
        public abstract Dictionary<Type, ICollection<IField>> FieldInfo { get; }
    }
}