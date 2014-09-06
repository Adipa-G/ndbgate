using System;

namespace DbGate
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableInfo : Attribute
    {
        public string TableName;
        public UpdateStrategy UpdateStrategy;
        public VerifyOnWriteStrategy VerifyOnWriteStrategy;
        public DirtyCheckStrategy DirtyCheckStrategy;

        public TableInfo(string tableName)
        {
            TableName = tableName;
        }
    }
}