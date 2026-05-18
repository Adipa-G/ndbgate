using System.Data;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.AccessDm
{
    public class AccessDataManipulate : AbstractDataManipulate
    {
        public AccessDataManipulate(IDbLayer dbLayer) : base(dbLayer)
        {
        }

        public override object ReadFromResultSet(IDataReader reader, IColumn column, int ordinal)
        {
            var result = base.ReadFromResultSet(reader, column, ordinal);
            if (result != null
                && column.ColumnType == ColumnType.Varchar)
            {
                return result.ToString().Replace("\u0000", "");
            }
            return result;
        }
    }
}