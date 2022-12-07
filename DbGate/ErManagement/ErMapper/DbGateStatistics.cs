using System;
using System.Collections;

namespace DbGate.ErManagement.ErMapper
{
    public class DbGateStatistics : IDbGateStatistics
    {
        private Hashtable deleteCount;
        private Hashtable inertCount;
        private Hashtable selectCount;
        private Hashtable updateCount;

        public DbGateStatistics()
        {
            Reset();
        }

        #region IDbGateStatistics Members

        public void Reset()
        {
            SelectQueryCount = 0;
            InsertQueryCount = 0;
            UpdateQueryCount = 0;
            DbPatchQueryCount = 0;
            DeleteQueryCount = 0;
            selectCount = new Hashtable();
            inertCount = new Hashtable();
            updateCount = new Hashtable();
            deleteCount = new Hashtable();
        }

        public int SelectQueryCount { get; set; }

        public int InsertQueryCount { get; set; }

        public int UpdateQueryCount { get; set; }

        public int DbPatchQueryCount { get; set; }

        public int DeleteQueryCount { get; set; }

        public int GetSelectCount(Type type)
        {
            return GetTypeCount(type, selectCount);
        }

        public int GetInsertCount(Type type)
        {
            return GetTypeCount(type, inertCount);
        }

        public int GetUpdateCount(Type type)
        {
            return GetTypeCount(type, updateCount);
        }

        public int GetDeleteCount(Type type)
        {
            return GetTypeCount(type, deleteCount);
        }

        public void RegisterSelect(Type type)
        {
            SelectQueryCount++;
            RegisterCount(type, selectCount);
        }

        public void RegisterInsert(Type type)
        {
            InsertQueryCount++;
            RegisterCount(type, inertCount);
        }

        public void RegisterUpdate(Type type)
        {
            UpdateQueryCount++;
            RegisterCount(type, updateCount);
        }

        public void RegisterDelete(Type type)
        {
            DeleteQueryCount++;
            RegisterCount(type, deleteCount);
        }

        public void RegisterPatch()
        {
            DbPatchQueryCount++;
        }

        #endregion

        private int GetTypeCount(Type type, Hashtable typeCountMap)
        {
            if (typeCountMap.ContainsKey(type))
            {
                return (int) typeCountMap[type];
            }
            return 0;
        }

        private void RegisterCount(Type type, Hashtable typeCountMap)
        {
            if (typeCountMap.ContainsKey(type))
            {
                var currentCount = (int) typeCountMap[type];
                currentCount ++;
                typeCountMap.Remove(type);
                typeCountMap.Add(type, currentCount);
            }
            else
            {
                typeCountMap.Add(type, 1);
            }
        }
    }
}