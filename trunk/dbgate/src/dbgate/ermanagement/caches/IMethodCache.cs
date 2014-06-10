using System;
using System.Reflection;

namespace dbgate.ermanagement.caches
{
    public interface IMethodCache
    {
        PropertyInfo GetProperty(Type entityType, string propertyName);

        void Clear();
    }
}