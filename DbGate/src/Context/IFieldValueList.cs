using System.Collections.Generic;

namespace DbGate.Context
{
    public interface IFieldValueList
    {
        ICollection<EntityFieldValue> FieldValues { get; }

        EntityFieldValue GetFieldValue(string attributeName);
    }
}