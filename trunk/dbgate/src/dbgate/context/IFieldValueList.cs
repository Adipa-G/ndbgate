using System.Collections.Generic;

namespace dbgate.context
{
    public interface IFieldValueList
    {
        ICollection<EntityFieldValue> FieldValues { get; }

        EntityFieldValue GetFieldValue(string attributeName);
    }
}