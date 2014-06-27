namespace DbGate.Support.Query.Basic
{
    [TableInfo("query_basic_join")]
    public class QueryBasicJoinEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Varchar), Key = true)]
        public string Name { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string OverrideDescription { get; set; }
    }
}