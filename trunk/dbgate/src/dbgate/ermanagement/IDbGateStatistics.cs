using System;

namespace dbgate.ermanagement
{
    public interface IDbGateStatistics
    {
        void Reset();

        int SelectQueryCount { get; }

        int InsertQueryCount { get; }

        int UpdateQueryCount { get; }

        int DeleteQueryCount { get; }

        int DbPatchQueryCount { get; }

        int GetSelectCount(Type type);

        int GetInsertCount(Type type);

        int GetUpdateCount(Type type);

        int GetDeleteCount(Type type);

        void RegisterSelect(Type type);

        void RegisterInsert(Type type);

        void RegisterUpdate(Type type);

        void RegisterDelete(Type type);

        void RegisterPatch();
    }
}
