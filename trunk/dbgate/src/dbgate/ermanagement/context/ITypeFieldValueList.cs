using System;

namespace dbgate.ermanagement.context
{
    public interface ITypeFieldValueList : IFieldValueList
    {
        Type Type { get; }
    }
}
