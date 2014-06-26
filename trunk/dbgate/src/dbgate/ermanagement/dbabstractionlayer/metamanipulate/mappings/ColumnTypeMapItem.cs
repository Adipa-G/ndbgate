using System.Data;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.mappings
{
    public class ColumnTypeMapItem
    {
        public ColumnTypeMapItem()
        {
        }

        public ColumnTypeMapItem(string name, ColumnType id)
        {
            Name = name;
            ColumnType = id;
        }

        public ColumnTypeMapItem(string name, ColumnType columnType, string defaultNonNullValue) : this(name,columnType)
        {
            DefaultNonNullValue = defaultNonNullValue;
        }

        public string Name { get; set; }

        public ColumnType ColumnType { get; set; }

        public string DefaultNonNullValue { get; set; }

        public void SetTypeFromSqlType(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    ColumnType = ColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.AnsiStringFixedLength:
                    ColumnType = ColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
//                case DbType.Binary:
//                    break;
                case DbType.Boolean:
                    ColumnType = ColumnType.Boolean;
                    DefaultNonNullValue = "true";
                    break;
                case DbType.Byte:
                    ColumnType = ColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Currency:
                    ColumnType = ColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Date:
                    ColumnType = ColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.DateTime:
                    ColumnType = ColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.DateTime2:
                    ColumnType = ColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
//                case DbType.DateTimeOffset:
//                    break;
                case DbType.Decimal:
                    ColumnType = ColumnType.Float;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Double:
                    ColumnType = ColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Guid:
                    ColumnType = ColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.Int16:
                    ColumnType = ColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Int32:
                    ColumnType = ColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Int64:
                    ColumnType = ColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
//                case DbType.Object:
//                    break;
                case DbType.SByte:
                    ColumnType = ColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.Single:
                    ColumnType = ColumnType.Double;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.String:
                    ColumnType = ColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.StringFixedLength:
                    ColumnType = ColumnType.Varchar;
                    DefaultNonNullValue = "''";
                    break;
                case DbType.Time:
                    ColumnType = ColumnType.Date;
                    DefaultNonNullValue = "1981/10/12";
                    break;
                case DbType.UInt16:
                    ColumnType = ColumnType.Integer;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.UInt32:
                    ColumnType = ColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.UInt64:
                    ColumnType = ColumnType.Long;
                    DefaultNonNullValue = "0";
                    break;
                case DbType.VarNumeric:
                    ColumnType = ColumnType.Float;
                    DefaultNonNullValue = "0";
                    break;
//                case DbType.Xml:
//                    break;
            }
        }
    }
}
