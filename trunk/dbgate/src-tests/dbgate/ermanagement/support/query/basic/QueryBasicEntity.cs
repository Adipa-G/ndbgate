namespace dbgate.ermanagement.support.query.basic
{
    [TableInfo("query_basic")]
    public class QueryBasicEntity : DefaultEntity
    {
        public QueryBasicEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_basic2join", typeof(QueryBasicJoinEntity), new[] { "IdCol","Name" }, new[] { "IdCol","Name" })]
        public QueryBasicJoinEntity JoinEntity { get; set; }
    }
}