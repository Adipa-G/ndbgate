using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.columntest
{
    public class ColumnTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            List<IField> dbColumns = new List<IField>();

            if (type == typeof(ColumnTestEntityExts))
            {
                dbColumns.Add(new DefaultDbColumn("IdCol","id_col",true, DbColumnType.Integer,true,new PrimaryKeyGenerator()));
                dbColumns.Add(new DefaultDbColumn("LongNotNull",DbColumnType.Long));
                dbColumns.Add(new DefaultDbColumn("LongNull", DbColumnType.Long, true));
                dbColumns.Add(new DefaultDbColumn("BooleanNotNull",DbColumnType.Boolean));
                dbColumns.Add(new DefaultDbColumn("BooleanNull",DbColumnType.Boolean,true));
                dbColumns.Add(new DefaultDbColumn("CharNotNull",DbColumnType.Char));
                dbColumns.Add(new DefaultDbColumn("CharNull",DbColumnType.Char,true));
                dbColumns.Add(new DefaultDbColumn("IntNotNull",DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("IntNull",DbColumnType.Integer,true));
                dbColumns.Add(new DefaultDbColumn("DateNotNull",DbColumnType.Date));
                dbColumns.Add(new DefaultDbColumn("DateNull",DbColumnType.Date,true));
                dbColumns.Add(new DefaultDbColumn("DoubleNotNull",DbColumnType.Double));
                dbColumns.Add(new DefaultDbColumn("DoubleNull",DbColumnType.Double,true));
                dbColumns.Add(new DefaultDbColumn("FloatNotNull",DbColumnType.Float));
                dbColumns.Add(new DefaultDbColumn("FloatNull",DbColumnType.Float,true));
                dbColumns.Add(new DefaultDbColumn("TimestampNotNull",DbColumnType.Timestamp));
                dbColumns.Add(new DefaultDbColumn("TimestampNull",DbColumnType.Timestamp,true));
                dbColumns.Add(new DefaultDbColumn("VarcharNotNull",DbColumnType.Varchar));
                dbColumns.Add(new DefaultDbColumn("VarcharNull",DbColumnType.Varchar,true));
            }

            return dbColumns;
        }

        public static String GetTableNames(Type type)
        {
            String tableName = null;
            if (type == typeof(ColumnTestEntityExts))
            {
                tableName = "column_test_entity";
            }
            return tableName;
        }
    }
}
