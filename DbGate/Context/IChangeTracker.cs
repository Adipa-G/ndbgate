using System.Collections.Generic;

namespace DbGate.Context
{
    public interface IChangeTracker
    {
        IEnumerable<ITypeFieldValueList> ChildEntityKeys { get; }

        bool Valid { get; }

        EntityFieldValue GetFieldValue(string attributeName);

        void AddChildEntityKey(ITypeFieldValueList list);

        void AddFields(ICollection<EntityFieldValue> fieldValues);
    }
}