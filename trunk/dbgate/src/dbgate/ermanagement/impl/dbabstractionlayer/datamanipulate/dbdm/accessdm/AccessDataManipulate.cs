using System.Data;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.dbdm.accessdm
{
    public class AccessDataManipulate : AbstractDataManipulate
    {
        public AccessDataManipulate(IDbLayer dbLayer) : base(dbLayer)
        {
        }

        public override object ReadFromResultSet(IDataReader reader, IDbColumn dbColumn)
        {
            object result = base.ReadFromResultSet(reader, dbColumn);
            if (result != null
                && dbColumn.ColumnType == DbColumnType.Varchar)
            {
                return result.ToString().Replace("\u0000", "");
            }
            return result;
        }
    }
}