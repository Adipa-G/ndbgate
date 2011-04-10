using System.Reflection;

namespace dbgate.ermanagement.caches
{
    public interface IMethodCache
    {
        PropertyInfo GetProperty(object obj, string propertyName);

        void Clear();
    }
}