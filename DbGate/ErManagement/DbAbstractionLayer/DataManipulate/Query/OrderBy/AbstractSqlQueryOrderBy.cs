using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy
{
    public class AbstractSqlQueryOrderBy : IAbstractOrderBy
    {
        public string Sql { get; set; }

        #region IAbstractOrderBy Members

        public QueryOrderByExpressionType OrderByExpressionType
        {
            get { return QueryOrderByExpressionType.RawSql; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}