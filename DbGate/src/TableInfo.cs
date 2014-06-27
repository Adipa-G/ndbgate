using System;

namespace DbGate
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableInfo : Attribute
    {
        public string TableName;

        public TableInfo(string tableName)
        {
            TableName = tableName;
        }
    }
}