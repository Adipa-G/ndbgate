namespace dbgate.ermanagement.support.query.basic
{
    [DbTableInfo("query_basic")]
    public class QueryBasicEntity : DefaultServerDbClass
    {
        public QueryBasicEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }
    }
}