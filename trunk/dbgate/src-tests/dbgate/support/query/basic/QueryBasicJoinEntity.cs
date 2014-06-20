using dbgate.ermanagement;

namespace dbgate.support.query.basic
{
    [TableInfo("query_basic_join")]
    public class QueryBasicJoinEntity : DefaultEntity
    {
        public QueryBasicJoinEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [ColumnInfo((ColumnType.Varchar),Key = true)]
        public string Name { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string OverrideDescription { get; set; }
    }
}