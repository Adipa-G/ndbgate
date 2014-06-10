namespace dbgate.ermanagement.support.query.basic
{
    [DbTableInfo("query_basic_join")]
    public class QueryBasicJoinEntity : DefaultServerDbClass
    {
        public QueryBasicJoinEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [DbColumnInfo((DbColumnType.Varchar),Key = true)]
        public string Name { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string OverrideDescription { get; set; }
    }
}