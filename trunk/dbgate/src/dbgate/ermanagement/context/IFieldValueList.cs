using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.context
{
    public interface IFieldValueList
    {
        ICollection<EntityFieldValue> FieldValues { get; }

        EntityFieldValue GetFieldValue(string attributeName);
    }
}