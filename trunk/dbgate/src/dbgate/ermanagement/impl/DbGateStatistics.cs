using System;
using System.Collections;

namespace dbgate.ermanagement.impl
{
    public class DbGateStatistics : IDbGateStatistics
    {
        private Hashtable _selectCount;
        private Hashtable _inertCount;
        private Hashtable _updateCount;
        private Hashtable _deleteCount;

        public DbGateStatistics()
        {
            Reset();
        }

        public void Reset()
        {
            SelectQueryCount = 0;
            InsertQueryCount = 0;
            UpdateQueryCount = 0;
            DbPatchQueryCount = 0;
            DeleteQueryCount  = 0;
            _selectCount        = new Hashtable();
            _inertCount        = new Hashtable();
            _updateCount       = new Hashtable();
            _deleteCount       = new Hashtable();
        }

        public int SelectQueryCount { get; set; }

        public int InsertQueryCount { get; set; }
    
        public int UpdateQueryCount { get; set; }
    
        public int DbPatchQueryCount { get; set; }
    
        public int DeleteQueryCount { get; set; }

        public int GetSelectCount(Type type)
        {
            return GetTypeCount(type,_selectCount);
        }

        public int GetInsertCount(Type type)
        {
            return GetTypeCount(type,_inertCount);
        }

        public int GetUpdateCount(Type type)
        {
            return GetTypeCount(type,_updateCount);
        }

        public int GetDeleteCount(Type type)
        {
            return GetTypeCount(type,_deleteCount);
        }

        private int GetTypeCount(Type type,Hashtable typeCountMap)
        {
            if (typeCountMap.ContainsKey(type))
            {
                return (int)typeCountMap[type];
            }
            return 0;
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
            registerCount(type,_deleteCount);
        }

        private void registerCount(Type type,Hashtable typeCountMap)
        {
            if (typeCountMap.ContainsKey(type))
            {
                int currentCount = (int)typeCountMap[type];
                currentCount ++;
                typeCountMap.Remove(type);
                typeCountMap.Add(type,currentCount);    
            }
            else
            {
                typeCountMap.Add(type,1);   
            }
        }

        public void RegisterPatch()
        {
            DbPatchQueryCount++;
        }
    }
}
