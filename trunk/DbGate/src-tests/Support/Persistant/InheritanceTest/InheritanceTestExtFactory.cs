using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            ICollection<IField> fields = new List<IField>();

            if (type == typeof (InheritanceTestSuperEntityExt))
            {
                var idCol = new DefaultColumn("IdCol", true, false, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultColumn("Name", ColumnType.Varchar));
            }
            else if (type == typeof (InheritanceTestSubEntityAExt))
            {
                var idCol = new DefaultColumn("IdCol", true, false, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultColumn("NameA", ColumnType.Varchar));
            }
            else if (type == typeof (InheritanceTestSubEntityBExt))
            {
                var idCol = new DefaultColumn("IdCol", true, false, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                fields.Add(idCol);
                fields.Add(new DefaultColumn("NameB", ColumnType.Varchar));
            }
            return fields;
        }

        public static ITable GetTableInfo(Type type)
        {
            ITable table = null;
            if (type == typeof (InheritanceTestSuperEntityExt))
            {
                table = new DefaultTable("inheritance_test_super");
            }
            else if (type == typeof (InheritanceTestSubEntityAExt))
            {
                table = new DefaultTable("inheritance_test_suba");
            }
            else if (type == typeof (InheritanceTestSubEntityBExt))
            {
                table = new DefaultTable("inheritance_test_subb");
            }
            return table;
        }
    }
}