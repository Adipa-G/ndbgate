namespace dbgate.ermanagement.support.query.basic
{
    [DbTableInfo("query_basic_details")]
    public class QueryBasicDetailsEntity : DefaultServerDbClass
    {
        public QueryBasicDetailsEntity()
        {
        }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Description { get; set; }
    }
}