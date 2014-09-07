using System;

namespace DbGate
{
    public interface IDbGateStatistics
    {
        int SelectQueryCount { get; }

        int InsertQueryCount { get; }

        int UpdateQueryCount { get; }

        int DeleteQueryCount { get; }

        int DbPatchQueryCount { get; }
        void Reset();

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