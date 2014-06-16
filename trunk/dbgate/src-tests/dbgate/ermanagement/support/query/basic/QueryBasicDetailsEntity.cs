namespace dbgate.ermanagement.support.query.basic
{
    [TableInfo("query_basic_details")]
    public class QueryBasicDetailsEntity : DefaultEntity
    {
        public QueryBasicDetailsEntity()
        {
        }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public string Description { get; set; }
    }
}