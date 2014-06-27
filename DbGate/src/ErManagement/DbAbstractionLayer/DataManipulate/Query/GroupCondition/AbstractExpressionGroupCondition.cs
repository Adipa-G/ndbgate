using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public class AbstractExpressionGroupCondition : IAbstractGroupCondition
    {
        private readonly AbstractExpressionProcessor _processor;

        public AbstractExpressionGroupCondition()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public GroupConditionExpr Expr { get; set; }

        #region IAbstractGroupCondition Members

        public QueryGroupConditionExpressionType GroupConditionExpressionType
        {
            get { return QueryGroupConditionExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return _processor.Process(null, Expr.RootSegment, buildInfo, dbLayer);
        }

        #endregion
    }
}