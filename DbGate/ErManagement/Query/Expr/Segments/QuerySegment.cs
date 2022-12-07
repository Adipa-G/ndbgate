using DbGate.Exceptions;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class QuerySegment : BaseSegment
    {
        private readonly string alias;
        private readonly ISelectionQuery query;

        public QuerySegment(ISelectionQuery query, string alias)
        {
            this.query = query;
            this.alias = alias;
        }

        public override SegmentType SegmentType => SegmentType.Query;

        public string Alias => alias;

        public ISelectionQuery Query => query;

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Group:
                    throw new ExpressionParsingException("Cannot add field/value/query/group segments to field segment");
                case SegmentType.Merge:
                    segment.Add(this);
                    return segment;
                case SegmentType.Compare:
                    segment.Add(this);
                    return segment;
                default:
                    return this;
            }
        }
    }
}