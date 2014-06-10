using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.treetest
{
    public class TreeTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            ICollection<IField> fields = new List<IField>();

            if (type == typeof(TreeTestRootEntityExt))
            {
                fields.Add(new DefaultDbColumn("IdCol", true, false, DbColumnType.Integer));
                fields.Add(new DefaultDbColumn("Name",DbColumnType.Varchar));
                fields.Add(new DefaultDbRelation("One2ManyEntities","fk_root2one2manyent", typeof(TreeTestOne2ManyEntityExt)
                    ,new DbRelationColumnMapping[]{new DbRelationColumnMapping("IdCol","idCol")}));
                fields.Add(new DefaultDbRelation("One2OneEntity","fk_root2one2oneent", typeof(TreeTestOne2OneEntityExt)
                    ,new DbRelationColumnMapping[]{new DbRelationColumnMapping("IdCol","idCol")}));

            }
            else if (type == typeof(TreeTestOne2ManyEntityExt))
            {
                fields.Add(new DefaultDbColumn("IdCol",true,false,DbColumnType.Integer));
                fields.Add(new DefaultDbColumn("IndexNo", true, false, DbColumnType.Integer));
                fields.Add(new DefaultDbColumn("Name",DbColumnType.Varchar));
            }
            else if (type == typeof(TreeTestOne2OneEntityExt))
            {
                fields.Add(new DefaultDbColumn("IdCol", true, false, DbColumnType.Integer));
                fields.Add(new DefaultDbColumn("Name", DbColumnType.Varchar));
            }
            return fields;
        }

        public static string GetTableNames(Type type)
        {
            string tableName = null;
            if (type == typeof( TreeTestRootEntityExt))
            {
                tableName =  "tree_test_root";
            }
            else if (type == typeof(TreeTestOne2ManyEntityExt))
            {
                tableName =  "tree_test_one2many";
            }
            else if (type == typeof(TreeTestOne2OneEntityExt))
            {
                tableName =  "tree_test_one2one";
            }
            return tableName;
        }
    }
}
