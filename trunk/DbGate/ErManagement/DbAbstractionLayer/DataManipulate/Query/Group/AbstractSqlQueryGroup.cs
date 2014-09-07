using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public class AbstractSqlQueryGroup : IAbstractGroup
    {
        public string Sql { get; set; }

        #region IAbstractGroup Members

        public QueryGroupExpressionType GroupExpressionType
        {
            get { return QueryGroupExpressionType.RawSql; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}