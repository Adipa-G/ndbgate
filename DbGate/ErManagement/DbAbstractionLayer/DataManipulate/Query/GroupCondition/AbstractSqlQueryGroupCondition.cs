using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public class AbstractSqlQueryGroupCondition : IAbstractGroupCondition
    {
        public string Sql { get; set; }

        #region IAbstractGroupCondition Members

        public QueryGroupConditionExpressionType GroupConditionExpressionType => QueryGroupConditionExpressionType.RawSql;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}