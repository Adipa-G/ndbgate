using dbgate.ermanagement.query.segments.condition;
using dbgate.ermanagement.query.segments.@from;
using dbgate.ermanagement.query.segments.group;

namespace dbgate.ermanagement.query
{
    public class QueryGroup
    {
		public static SqlQueryGroup RawSql(string sql)
        {
            return new SqlQueryGroup(sql);
        }
    }
}
