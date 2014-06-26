using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.condition
{
    public class AbstractExpressionCondition : IAbstractCondition
    {
        private AbstractExpressionProcessor _processor;

        public AbstractExpressionCondition()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public ConditionExpr Expr { get; set; }
        
        public QueryConditionExpressionType ConditionExpressionType
        {
            get { return QueryConditionExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return _processor.Process(null, Expr.RootSegment, buildInfo,dbLayer);
        }
    }
}
