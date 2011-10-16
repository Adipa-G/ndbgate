using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.mappings
{
    public class ColumnTypeMapItem
    {
        public ColumnTypeMapItem()
        {
        }

        public ColumnTypeMapItem(string name, DbColumnType id)
        {
            Name = name;
            ColumnType = id;
        }

        public ColumnTypeMapItem(string name, DbColumnType columnType, string defaultNonNullValue) : this(name,columnType)
        {
            DefaultNonNullValue = defaultNonNullValue;
        }

        public string Name { get; set; }

        public DbColumnType ColumnType { get; set; }

        public string DefaultNonNullValue { get; set; }

        public void SetTypeFromSqlType(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    ColumnType = DbColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.AnsiStringFixedLength:
                    ColumnType = DbColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
//                case DbType.Binary:
//                    break;
                case DbType.Boolean:
                    ColumnType = DbColumnType.Boolean;
                    DefaultNonNullValue = "true";
                    break;
                case DbType.Byte:
                    ColumnType = DbColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Currency:
                    ColumnType = DbColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Date:
                    ColumnType = DbColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.DateTime:
                    ColumnType = DbColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.DateTime2:
                    ColumnType = DbColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
//                case DbType.DateTimeOffset:
//                    break;
                case DbType.Decimal:
                    ColumnType = DbColumnType.Float;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Double:
                    ColumnType = DbColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Guid:
                    ColumnType = DbColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.Int16:
                    ColumnType = DbColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Int32:
                    ColumnType = DbColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Int64:
                    ColumnType = DbColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
//                case DbType.Object:
//                    break;
                case DbType.SByte:
                    ColumnType = DbColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Single:
                    ColumnType = DbColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.String:
                    ColumnType = DbColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.StringFixedLength:
                    ColumnType = DbColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.Time:
                    ColumnType = DbColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.UInt16:
                    ColumnType = DbColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.UInt32:
                    ColumnType = DbColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.UInt64:
                    ColumnType = DbColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.VarNumeric:
                    ColumnType = DbColumnType.Float;
                    DefaultNonNullValue = "0";
                    break;
//                case DbType.Xml:
//                    break;
            }
        }
    }
}
