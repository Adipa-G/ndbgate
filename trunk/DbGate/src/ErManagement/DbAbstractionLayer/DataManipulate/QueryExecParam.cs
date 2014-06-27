namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate
{
    public class QueryExecParam
    {
        public int Index { get; set; }

        public ColumnType Type { get; set; }

        public object Value { get; set; }
    }
}