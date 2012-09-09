using dbgate.ermanagement.query.segments.condition;
using dbgate.ermanagement.query.segments.@from;

namespace dbgate.ermanagement.query
{
    public class QueryFrom
    {
        public static SqlQueryFrom RawSql(string sql)
        {
            return new SqlQueryFrom(sql);
        }
    }
}
