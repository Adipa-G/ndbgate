namespace dbgate.ermanagement.query.segments
{
    public class SqlSegment
    {
        public SqlSegment(string sql)
        {
            Sql = sql;
        }

        public string Sql { get; set; }
    }
}
