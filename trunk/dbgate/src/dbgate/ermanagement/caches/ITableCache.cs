using System;

namespace dbgate.ermanagement.caches
{
    public interface ITableCache
    {
        string GetTableName(Type type);

        void Register(Type type, string tableName);

        void Register(Type type, IServerRoDbClass serverRoDbClass);

        void Clear();
    }
}