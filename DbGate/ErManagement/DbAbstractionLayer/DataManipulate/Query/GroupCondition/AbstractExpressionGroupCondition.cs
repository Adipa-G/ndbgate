using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public class AbstractExpressionGroupCondition : IAbstractGroupCondition
    {
        private readonly AbstractExpressionProcessor processor;

        public AbstractExpressionGroupCondition()
        {
            processor = new AbstractExpressionProcessor();
        }

        public GroupConditionExpr Expr { get; set; }

        #region IAbstractGroupCondition Members

        public QueryGroupConditionExpressionType GroupConditionExpressionType => QueryGroupConditionExpressionType.Expression;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return processor.Process(null, Expr.RootSegment, buildInfo, dbLayer);
        }

        #endregion
    }
}