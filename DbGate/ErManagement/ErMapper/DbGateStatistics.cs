using System;
using System.Collections;

namespace DbGate.ErManagement.ErMapper
{
    public class DbGateStatistics : IDbGateStatistics
    {
        private Hashtable _deleteCount;
        private Hashtable _inertCount;
        private Hashtable _selectCount;
        private Hashtable _updateCount;

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
            _selectCount = new Hashtable();
            _inertCount = new Hashtable();
            _updateCount = new Hashtable();
            _deleteCount = new Hashtable();
        }

        public int SelectQueryCount { get; set; }

        public int InsertQueryCount { get; set; }

        public int UpdateQueryCount { get; set; }

        public int DbPatchQueryCount { get; set; }

        public int DeleteQueryCount { get; set; }

        public int GetSelectCount(Type type)
        {
            return GetTypeCount(type, _selectCount);
        }

        public int GetInsertCount(Type type)
        {
            return GetTypeCount(type, _inertCount);
        }

        public int GetUpdateCount(Type type)
        {
            return GetTypeCount(type, _updateCount);
        }

        public int GetDeleteCount(Type type)
        {
            return GetTypeCount(type, _deleteCount);
        }

        public void RegisterSelect(Type type)
        {
            SelectQueryCount++;
            registerCount(type, _selectCount);
        }

        public void RegisterInsert(Type type)
        {
            InsertQueryCount++;
            registerCount(type, _inertCount);
        }

        public void RegisterUpdate(Type type)
        {
            UpdateQueryCount++;
            registerCount(type, _updateCount);
        }

        public void RegisterDelete(Type type)
        {
            DeleteQueryCount++;
            registerCount(type, _deleteCount);
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

        private void registerCount(Type type, Hashtable typeCountMap)
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