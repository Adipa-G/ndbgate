using System;

namespace dbgate.context
{
    public interface ITypeFieldValueList : IFieldValueList
    {
        Type Type { get; }
    }
}
