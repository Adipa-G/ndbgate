using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.groupcondition
{
    public class AbstractExpressionGroupCondition : IAbstractGroupCondition
    {
        private AbstractExpressionProcessor _processor;
        public GroupConditionExpr Expr { get; set; }

        public AbstractExpressionGroupCondition()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public QueryGroupConditionExpressionType GroupConditionExpressionType
        {
            get { return QueryGroupConditionExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return _processor.Process(null, Expr.RootSegment, buildInfo, dbLayer);
        }
    }
	
}

