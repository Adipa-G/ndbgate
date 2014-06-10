using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class QuerySegment : BaseSegment
    {
        private readonly string _alias;
        private readonly ISelectionQuery _query;

        public QuerySegment(ISelectionQuery query, string alias)
        {
            this._query = query;
            this._alias = alias;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Query; }
        }

        public string Alias
        {
            get { return _alias; }
        }

        public ISelectionQuery Query
        {
            get { return _query; }
        }

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
