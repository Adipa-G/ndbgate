using System.Collections.Generic;

namespace dbgate.ermanagement.context
{
    public interface IChangeTracker
    {
        ICollection<EntityFieldValue> Fields { get; }

        ICollection<ITypeFieldValueList> ChildEntityKeys { get; }

        EntityFieldValue GetFieldValue(string attributeName);

        bool Valid { get; }
    }
}
