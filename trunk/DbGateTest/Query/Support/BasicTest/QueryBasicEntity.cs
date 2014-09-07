namespace DbGate.Query.Support.BasicTest
{
    [TableInfo("query_basic")]
    public class QueryBasicEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_basic2join", typeof (QueryBasicJoinEntity), new[] {"IdCol", "Name"}, new[] {"IdCol", "Name"}
            )]
        public QueryBasicJoinEntity JoinEntity { get; set; }
    }
}