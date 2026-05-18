using System;
using System.Data;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.SqliteDm
{
    public class SqliteDataManipulate : AbstractDataManipulate
    {
        public SqliteDataManipulate(IDbLayer dbLayer) : base(dbLayer)
        {
        }

        public override object ReadFromResultSet(IDataReader reader, IColumn column, int ordinal)
        {
            if (column.ColumnType == ColumnType.Guid)
            {
                var columnCopy = column.Clone();
                columnCopy.ColumnType = ColumnType.Varchar;
                var result = (string)base.ReadFromResultSet(reader, columnCopy, ordinal);

                var guidResult = Guid.Empty;
                var success = Guid.TryParse(result, out guidResult);

                return (result == null || !success) ?
                    (column.Nullable ? null : (object)Guid.Empty) :
                    guidResult;
            }
            return base.ReadFromResultSet(reader, column, ordinal);
        }

        protected override void SetToPreparedStatement(IDbCommand cmd, object obj, int parameterIndex, bool nullable, ColumnType columnType)
        {
            if (columnType == ColumnType.Guid)
            {
                var value = obj?.ToString() ?? obj;
                base.SetToPreparedStatement(cmd, value, parameterIndex, nullable, ColumnType.Varchar);
            }
            else
            {
                base.SetToPreparedStatement(cmd, obj, parameterIndex, nullable, columnType);
            }
        }
    }
}