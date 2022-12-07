using System;
using DbGate.Exceptions;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class GroupFunctionSegment : BaseSegment
    {
        private readonly string custFunction;
        private readonly GroupFunctionSegmentMode groupFunctionMode;

        public GroupFunctionSegment(GroupFunctionSegmentMode groupFunctionMode)
        {
            this.groupFunctionMode = groupFunctionMode;
            custFunction = null;
        }

        public GroupFunctionSegment(String custFunction)
        {
            groupFunctionMode = GroupFunctionSegmentMode.CustFunc;
            this.custFunction = custFunction;
        }

        public FieldSegment SegmentToGroup { get; set; }

        public override SegmentType SegmentType => SegmentType.Group;

        public GroupFunctionSegmentMode GroupFunctionMode => groupFunctionMode;

        public string CustFunction => custFunction;

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