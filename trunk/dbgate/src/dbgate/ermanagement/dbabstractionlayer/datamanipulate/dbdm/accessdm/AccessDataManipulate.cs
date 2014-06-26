using System.Data;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.dbdm.accessdm
{
    public class AccessDataManipulate : AbstractDataManipulate
    {
        public AccessDataManipulate(IDbLayer dbLayer) : base(dbLayer)
        {
        }

        public override object ReadFromResultSet(IDataReader reader, IColumn column)
        {
            object result = base.ReadFromResultSet(reader, column);
            if (result != null
                && column.ColumnType == ColumnType.Varchar)
            {
                return result.ToString().Replace("\u0000", "");
            }
            return result;
        }
    }
}