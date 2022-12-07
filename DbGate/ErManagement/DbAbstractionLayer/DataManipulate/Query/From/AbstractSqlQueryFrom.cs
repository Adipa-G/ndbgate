using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public class AbstractSqlQueryFrom : IAbstractFrom
    {
        public string Sql { get; set; }

        #region IAbstractFrom Members

        public QueryFromExpressionType FromExpressionType => QueryFromExpressionType.RawSql;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}