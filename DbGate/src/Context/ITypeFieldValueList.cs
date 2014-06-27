using System;

namespace DbGate.Context
{
    public interface ITypeFieldValueList : IFieldValueList
    {
        Type Type { get; }
    }
}