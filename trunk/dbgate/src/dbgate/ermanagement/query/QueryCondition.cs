using dbgate.ermanagement.query.segments.condition;

namespace dbgate.ermanagement.query
{
    public class QueryCondition
    {
        public static SqlQueryCondition RawSql(string sql)
        {
            return new SqlQueryCondition(sql);
        }
    }
}
