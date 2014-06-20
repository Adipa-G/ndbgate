using System;
using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.treetest
{
    public class TreeTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            ICollection<IField> fields = new List<IField>();

            if (type == typeof(TreeTestRootEntityExt))
            {
                fields.Add(new DefaultColumn("IdCol", true, false, ColumnType.Integer));
                fields.Add(new DefaultColumn("Name",ColumnType.Varchar));
                fields.Add(new DefaultRelation("One2ManyEntities","fk_root2one2manyent", typeof(TreeTestOne2ManyEntityExt)
                    ,new RelationColumnMapping[]{new RelationColumnMapping("IdCol","idCol")}));
                fields.Add(new DefaultRelation("One2OneEntity","fk_root2one2oneent", typeof(TreeTestOne2OneEntityExt)
                    ,new RelationColumnMapping[]{new RelationColumnMapping("IdCol","idCol")}));

            }
            else if (type == typeof(TreeTestOne2ManyEntityExt))
            {
                fields.Add(new DefaultColumn("IdCol",true,false,ColumnType.Integer));
                fields.Add(new DefaultColumn("IndexNo", true, false, ColumnType.Integer));
                fields.Add(new DefaultColumn("Name",ColumnType.Varchar));
            }
            else if (type == typeof(TreeTestOne2OneEntityExt))
            {
                fields.Add(new DefaultColumn("IdCol", true, false, ColumnType.Integer));
                fields.Add(new DefaultColumn("Name", ColumnType.Varchar));
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
