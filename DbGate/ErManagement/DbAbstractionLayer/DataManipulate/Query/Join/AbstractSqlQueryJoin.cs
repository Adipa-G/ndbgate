using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join
{
    public class AbstractSqlQueryJoin : IAbstractJoin
    {
        public string Sql { get; set; }

        #region IAbstractJoin Members

        public QueryJoinExpressionType JoinExpressionType => QueryJoinExpressionType.RawSql;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}