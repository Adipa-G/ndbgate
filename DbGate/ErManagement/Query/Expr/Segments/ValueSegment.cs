using System;
using DbGate.Exceptions;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class ValueSegment : BaseSegment
    {
        private readonly ColumnType type;
        private readonly object[] values;

        public ValueSegment(ColumnType type, object[] values)
        {
            this.type = type;
            this.values = values;
        }

        public ValueSegment(object[] values)
        {
            this.values = values;
            var valueType = this.values[0].GetType();
            type = ColumnTypeMapping.GetColumnType(valueType);
        }

        public override SegmentType SegmentType => SegmentType.Value;

        public ColumnType Type => type;

        public object[] Values => values;

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Group:
                    throw new ExpressionParsingException(
                        "Cannot add field/value/query/merge/group segments to value segment");
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