using System;
using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class GroupFunctionSegment : BaseSegment
    {
        private readonly GroupFunctionSegmentMode _groupFunctionMode;
        private readonly string _custFunction;

        public FieldSegment SegmentToGroup { get; set; }

        public GroupFunctionSegment(GroupFunctionSegmentMode groupFunctionMode)
        {
            _groupFunctionMode = groupFunctionMode;
            _custFunction = null;
        }

        public GroupFunctionSegment(String custFunction)
        {
            _groupFunctionMode = GroupFunctionSegmentMode.CustFunc;
            _custFunction = custFunction;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Group; }
        }

        public GroupFunctionSegmentMode GroupFunctionMode
        {
            get { return _groupFunctionMode; }
        }

        public string CustFunction
        {
            get { return _custFunction; }
        }

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                    SegmentToGroup = (FieldSegment) segment;
                    return this;
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Merge:
                case SegmentType.Group:
                    throw new ExpressionParsingException("Cannot add value/query/merge/group segments to field segment");
                case SegmentType.Compare:
                    segment.Add(this);
                    return segment;
                default:
                    return this;
            }
        }
    }
}
