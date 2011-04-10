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

        public string Name { get; set; }

        public DbColumnType ColumnType { get; set; }

        public void SetTypeFromSqlType(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    ColumnType = DbColumnType.Varchar;
                    break;
                case DbType.AnsiStringFixedLength:
                    ColumnType = DbColumnType.Varchar;
                    break;
//                case DbType.Binary:
//                    break;
                case DbType.Boolean:
                    ColumnType = DbColumnType.Boolean;
                    break;
                case DbType.Byte:
                    ColumnType = DbColumnType.Integer;
                    break;
                case DbType.Currency:
                    ColumnType = DbColumnType.Double;
                    break;
                case DbType.Date:
                    ColumnType = DbColumnType.Date;
                    break;
                case DbType.DateTime:
                    ColumnType = DbColumnType.Date;
                    break;
                case DbType.DateTime2:
                    ColumnType = DbColumnType.Date;
                    break;
//                case DbType.DateTimeOffset:
//                    break;
                case DbType.Decimal:
                    ColumnType = DbColumnType.Float;
                    break;
                case DbType.Double:
                    ColumnType = DbColumnType.Double;
                    break;
                case DbType.Guid:
                    ColumnType = DbColumnType.Varchar;
                    break;
                case DbType.Int16:
                    ColumnType = DbColumnType.Integer;
                    break;
                case DbType.Int32:
                    ColumnType = DbColumnType.Long;
                    break;
                case DbType.Int64:
                    ColumnType = DbColumnType.Long;
                    break;
//                case DbType.Object:
//                    break;
                case DbType.SByte:
                    ColumnType = DbColumnType.Integer;
                    break;
                case DbType.Single:
                    ColumnType = DbColumnType.Double;
                    break;
                case DbType.String:
                    ColumnType = DbColumnType.Varchar;
                    break;
                case DbType.StringFixedLength:
                    ColumnType = DbColumnType.Varchar;
                    break;
                case DbType.Time:
                    ColumnType = DbColumnType.Date;
                    break;
                case DbType.UInt16:
                    ColumnType = DbColumnType.Integer;
                    break;
                case DbType.UInt32:
                    ColumnType = DbColumnType.Long;
                    break;
                case DbType.UInt64:
                    ColumnType = DbColumnType.Long;
                    break;
                case DbType.VarNumeric:
                    ColumnType = DbColumnType.Float;
                    break;
//                case DbType.Xml:
//                    break;
            }
        }
    }
}
