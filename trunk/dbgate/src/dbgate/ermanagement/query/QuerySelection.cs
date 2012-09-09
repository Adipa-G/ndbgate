using dbgate.ermanagement.query.segments.condition;
using dbgate.ermanagement.query.segments.@from;
using dbgate.ermanagement.query.segments.selection;

namespace dbgate.ermanagement.query
{
    public class QuerySelection
    {
        public static IQuerySelection RawSql(string sql)
        {
            return new SqlQuerySelection(sql);
        }
    }
}
