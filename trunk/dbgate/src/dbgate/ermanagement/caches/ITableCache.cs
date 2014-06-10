using System;

namespace dbgate.ermanagement.caches
{
    public interface ITableCache
    {
        string GetTableName(Type entityType);

        void Register(Type entityType, string tableName);

        void Register(Type entityType);

        void Clear();
    }
}