using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public class InheritanceTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            ICollection<IField> fields = new List<IField>();

            if (type == typeof(InheritanceTestSuperEntityExt))
            {
                DefaultDbColumn idCol = new DefaultDbColumn("IdCol",true,false,DbColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultDbColumn("Name",DbColumnType.Varchar));
            }
            else if (type == typeof(InheritanceTestSubEntityAExt))
            {
                DefaultDbColumn idCol = new DefaultDbColumn("IdCol",true,false,DbColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultDbColumn("NameA",DbColumnType.Varchar));
            }
            else if (type == typeof(InheritanceTestSubEntityBExt))
            {
                DefaultDbColumn idCol = new DefaultDbColumn("IdCol", true, false, DbColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultDbColumn("NameB",DbColumnType.Varchar));
            }
            return fields;
        }

        public static string GetTableNames(Type type)
        {
            string tableName = null;
            if (type == typeof(InheritanceTestSuperEntityExt))
            {
                tableName =  "inheritance_test_super";
            }
            else if (type == typeof(InheritanceTestSubEntityAExt))
            {
                tableName =  "inheritance_test_suba";
            }
            else if (type == typeof(InheritanceTestSubEntityBExt))
            {
                tableName =  "inheritance_test_subb";
            }
            return tableName;
        }
    }
}
