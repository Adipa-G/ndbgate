using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.ErManagement.Query.Expr.Segments;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public class AbstractExpressionGroup : IAbstractGroup
    {
        private readonly AbstractExpressionProcessor processor;

        public AbstractExpressionGroup()
        {
            processor = new AbstractExpressionProcessor();
        }

        public GroupExpr Expr { get; set; }

        #region IAbstractGroup Members

        public QueryGroupExpressionType GroupExpressionType => QueryGroupExpressionType.Expression;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            var rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Field:
                    return processor.GetFieldName((FieldSegment) rootSegment, false, buildInfo);
            }
            return null;
        }

        #endregion
    }
}