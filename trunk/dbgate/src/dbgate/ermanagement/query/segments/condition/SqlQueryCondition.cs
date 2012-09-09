namespace dbgate.ermanagement.query.segments.condition
{
    public class SqlQueryCondition : SqlSegment,IQueryCondition
    {
        public SqlQueryCondition(string sql) : base(sql)
        {
        }
    }
}
