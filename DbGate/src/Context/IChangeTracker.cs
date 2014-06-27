using System.Collections.Generic;

namespace DbGate.Context
{
    public interface IChangeTracker
    {
        ICollection<EntityFieldValue> Fields { get; }

        ICollection<ITypeFieldValueList> ChildEntityKeys { get; }

        bool Valid { get; }
        EntityFieldValue GetFieldValue(string attributeName);
    }
}