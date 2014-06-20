using System;
using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.columntest
{
    public class ColumnTestExtFactory
    {
        public static ICollection<IField> GetFieldInfo(Type type)
        {
            List<IField> dbColumns = new List<IField>();

            if (type == typeof(ColumnTestEntityExts))
            {
                dbColumns.Add(new DefaultColumn("IdCol","id_col",true, ColumnType.Integer,true,new PrimaryKeyGenerator()));
                dbColumns.Add(new DefaultColumn("LongNotNull",ColumnType.Long));
                dbColumns.Add(new DefaultColumn("LongNull", ColumnType.Long, true));
                dbColumns.Add(new DefaultColumn("BooleanNotNull",ColumnType.Boolean));
                dbColumns.Add(new DefaultColumn("BooleanNull",ColumnType.Boolean,true));
                dbColumns.Add(new DefaultColumn("CharNotNull",ColumnType.Char));
                dbColumns.Add(new DefaultColumn("CharNull",ColumnType.Char,true));
                dbColumns.Add(new DefaultColumn("IntNotNull",ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("IntNull",ColumnType.Integer,true));
                dbColumns.Add(new DefaultColumn("DateNotNull",ColumnType.Date));
                dbColumns.Add(new DefaultColumn("DateNull",ColumnType.Date,true));
                dbColumns.Add(new DefaultColumn("DoubleNotNull",ColumnType.Double));
                dbColumns.Add(new DefaultColumn("DoubleNull",ColumnType.Double,true));
                dbColumns.Add(new DefaultColumn("FloatNotNull",ColumnType.Float));
                dbColumns.Add(new DefaultColumn("FloatNull",ColumnType.Float,true));
                dbColumns.Add(new DefaultColumn("TimestampNotNull",ColumnType.Timestamp));
                dbColumns.Add(new DefaultColumn("TimestampNull",ColumnType.Timestamp,true));
                dbColumns.Add(new DefaultColumn("VarcharNotNull",ColumnType.Varchar));
                dbColumns.Add(new DefaultColumn("VarcharNull",ColumnType.Varchar,true));
            }

            return dbColumns;
        }

        public static string GetTableNames(Type type)
        {
            string tableName = null;
            if (type == typeof(ColumnTestEntityExts))
            {
                tableName = "column_test_entity";
            }
            return tableName;
        }
    }
}
