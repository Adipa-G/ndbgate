using dbgate.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class ValueSegment : BaseSegment
    {
        private readonly ColumnType _type;
        private readonly object[] _values;

        public ValueSegment(ColumnType type, object[] values)
        {
            this._type = type;
            this._values = values;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Value;}
        }

        public ColumnType Type
        {
            get { return _type; }
        }

        public object[] Values
        {
            get { return _values; }
        }

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Group:
                    throw new ExpressionParsingException("Cannot add field/value/query/merge/group segments to value segment");
                case SegmentType.Merge:
                    segment.Add(this);
                    return segment;
                case SegmentType.Compare:
                    segment.Add(this);
                    Parent = segment;
                    return segment;
                default:
                    return this;
            }
        }
    }
}
