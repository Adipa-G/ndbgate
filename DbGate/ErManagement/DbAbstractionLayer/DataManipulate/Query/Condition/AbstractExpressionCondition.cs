using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition
{
    public class AbstractExpressionCondition : IAbstractCondition
    {
        private readonly AbstractExpressionProcessor processor;

        public AbstractExpressionCondition()
        {
            processor = new AbstractExpressionProcessor();
        }

        public ConditionExpr Expr { get; set; }

        #region IAbstractCondition Members

        public QueryConditionExpressionType ConditionExpressionType => QueryConditionExpressionType.Expression;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return processor.Process(null, Expr.RootSegment, buildInfo, dbLayer);
        }

        #endregion
    }
}