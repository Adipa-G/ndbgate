namespace DbGate.Query.Support.BasicTest
{
    [TableInfo("query_basic_details")]
    public class QueryBasicDetailsEntity : DefaultEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Description { get; set; }
    }
}