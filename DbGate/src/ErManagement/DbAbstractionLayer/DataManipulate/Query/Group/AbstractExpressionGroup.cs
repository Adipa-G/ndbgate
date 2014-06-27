using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.ErManagement.Query.Expr.Segments;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public class AbstractExpressionGroup : IAbstractGroup
    {
        private readonly AbstractExpressionProcessor _processor;

        public AbstractExpressionGroup()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public GroupExpr Expr { get; set; }

        #region IAbstractGroup Members

        public QueryGroupExpressionType GroupExpressionType
        {
            get { return QueryGroupExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            ISegment rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Field:
                    return _processor.GetFieldName((FieldSegment) rootSegment, false, buildInfo);
            }
            return null;
        }

        #endregion
    }
}