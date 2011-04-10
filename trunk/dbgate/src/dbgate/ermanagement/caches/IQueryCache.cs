using System;

namespace dbgate.ermanagement.caches
{
    public interface IQueryCache
    {
        string GetLoadQuery(Type entityType);

        string GetInsertQuery(Type entityType);

        string GetUpdateQuery(Type entityType);

        string GetDeleteQuery(Type entityType);

        string GetRelationObjectLoad(Type entityType,IDbRelation relation);

        void Clear();
    }
}