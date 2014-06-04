using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class ValueSegment : BaseSegment
    {
        private readonly DbColumnType _type;
        private readonly object _value;

        public ValueSegment(DbColumnType type, object value)
        {
            this._type = type;
            if (!(value is object[]))
            {
                value = new[]{value};
            }
            this._value = value;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Value;}
        }

        public DbColumnType Type
        {
            get { return _type; }
        }

        public object Value
        {
            get { return _value; }
        }

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Group:
                    throw new ExpressionParsingError("Cannot add field/value/query/merge/group segments to value segment");
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
