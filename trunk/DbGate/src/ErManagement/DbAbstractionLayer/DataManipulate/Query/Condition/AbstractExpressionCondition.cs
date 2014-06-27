using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition
{
    public class AbstractExpressionCondition : IAbstractCondition
    {
        private readonly AbstractExpressionProcessor _processor;

        public AbstractExpressionCondition()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public ConditionExpr Expr { get; set; }

        #region IAbstractCondition Members

        public QueryConditionExpressionType ConditionExpressionType
        {
            get { return QueryConditionExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return _processor.Process(null, Expr.RootSegment, buildInfo, dbLayer);
        }

        #endregion
    }
}