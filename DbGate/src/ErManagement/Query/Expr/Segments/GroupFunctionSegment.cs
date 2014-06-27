using System;
using DbGate.Exceptions;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class GroupFunctionSegment : BaseSegment
    {
        private readonly string _custFunction;
        private readonly GroupFunctionSegmentMode _groupFunctionMode;

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

        public FieldSegment SegmentToGroup { get; set; }

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