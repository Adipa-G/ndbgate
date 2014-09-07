using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition
{
    public class AbstractSqlQueryCondition : IAbstractCondition
    {
        public string Sql { get; set; }

        #region IAbstractCondition Members

        public QueryConditionExpressionType ConditionExpressionType
        {
            get { return QueryConditionExpressionType.RawSql; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        #endregion
    }
}