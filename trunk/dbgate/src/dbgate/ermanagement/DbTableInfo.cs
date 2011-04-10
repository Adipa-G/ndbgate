using System;

namespace dbgate.ermanagement
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbTableInfo : Attribute
    {
        public String TableName;

        public DbTableInfo(string tableName)
        {
            TableName = tableName;
        }
    }
}